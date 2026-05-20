using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TRIO2026.App.Services;
using TRIO2026.App.Views;
using TRIO2026.Core;
using TRIO2026.Core.Interfaces;
using TRIO2026.Data.Contexts;

namespace TRIO2026.App;

/// <summary>
/// WPF 應用程式入口 — DI 容器配置 + 啟動 AppShell（單一 Window）
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 全域 Dispatcher 例外處理（結構化日誌 + 顯示 ErrorId）
        DispatcherUnhandledException += (s, ex) =>
        {
            var errorId = ErrorCodes.UnhandledException;
            try
            {
                EventLogService.Instance?.LogException(
                    "System", "App", ex.Exception, errorId,
                    "Dispatcher 未處理例外");
            }
            catch { }

            var loc = LocalizationService.Instance;
            MessageBox.Show(
                $"Error ID: {errorId}\n\n" +
                $"{ex.Exception.Message}\n\n" +
                loc["Error.ReportHint"],
                loc["Error.Title"], MessageBoxButton.OK, MessageBoxImage.Error);
            ex.Handled = true;
        };

        // AppDomain 未處理例外
        AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
        {
            if (ex.ExceptionObject is Exception e2)
                EventLogService.Instance?.LogException(
                    "System", "App", e2, ErrorCodes.UnhandledException,
                    "AppDomain 未處理例外");
        };

        // ProcessExit — 工作管理員關閉、系統登出等非正常結束
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            Console.WriteLine("[App] ProcessExit detected — flushing logs");
            EventLogService.Instance?.LogInfo("System", "App", ErrorCodes.AppShutdown,
                "ProcessExit 偵測到程序結束");
            if (EventLogService.Instance is IDisposable d) d.Dispose();
        };

        // TaskScheduler 未觀察到的 Task 例外
        TaskScheduler.UnobservedTaskException += (s, ex) =>
        {
            EventLogService.Instance?.LogException(
                "System", "App", ex.Exception, ErrorCodes.UnhandledException,
                "Task 未觀察到的例外");
            ex.SetObserved();
        };

        try
        {
            // 取得專案根目錄（Database/ 所在位置）
            var baseDir = FindProjectRoot();
            var dbDir = Path.Combine(baseDir, "Database");

            // 確保 Database 目錄存在
            if (!Directory.Exists(dbDir))
                Directory.CreateDirectory(dbDir);

            // DI 容器
            var services = new ServiceCollection();


            // DbContext 註冊（新 DB）
            services.AddDbContext<SystemConfigDbContext>(options =>
                options.UseSqlite($"Data Source={Path.Combine(dbDir, "system_config.db")}"));
            services.AddDbContext<EventLogDbContext>(options =>
                options.UseSqlite($"Data Source={Path.Combine(dbDir, "system_event.db")}"));
            services.AddDbContext<AppMainDbContext>(options =>
                options.UseSqlite($"Data Source={Path.Combine(dbDir, "main.db")}"));

            // Services
            services.AddSingleton<SessionService>();
            services.AddSingleton<TokenService>();
            services.AddSingleton<SystemSettingService>();
            services.AddTransient<AuthService>();

            // UV 相關服務
            services.AddSingleton<UvConfigService>();
            services.AddSingleton<IUvHardwareService, MockUvHardwareService>();
            services.AddSingleton<LocalizationService>();
            services.AddSingleton<EventLogService>();
            services.AddSingleton<EventLogArchiveService>();

            _serviceProvider = services.BuildServiceProvider();

            // 確保資料庫 schema 為最新版
            using (var scope = _serviceProvider.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<SystemConfigDbContext>().Database.Migrate();
                scope.ServiceProvider.GetRequiredService<EventLogDbContext>().Database.Migrate();
                scope.ServiceProvider.GetRequiredService<AppMainDbContext>().Database.Migrate();
            }

            // 初始化事件日誌服務
            var eventLog = _serviceProvider.GetRequiredService<EventLogService>();
            eventLog.SessionService = _serviceProvider.GetRequiredService<SessionService>();
            EventLogService.Instance = eventLog;
            eventLog.LogInfo("System", "App", ErrorCodes.AppStartup, "應用程式啟動");

            // 啟動歸檔檢查
            var archiveService = _serviceProvider.GetRequiredService<EventLogArchiveService>();
            archiveService.CheckAndArchiveAsync().GetAwaiter().GetResult();


            // 載入系統設定（system_config.db）
            var sysSettings = _serviceProvider.GetRequiredService<SystemSettingService>();
            sysSettings.LoadAsync().GetAwaiter().GetResult();

            // 初始化多語系服務（受 DB 開關控制）
            var locService = _serviceProvider.GetRequiredService<LocalizationService>();
            var defaultLang = sysSettings.MultiLanguageEnabled ? sysSettings.DefaultLanguage : "en";
            locService.InitializeAsync(defaultLang).GetAwaiter().GetResult();

            // 解析模擬器參數
            var simArgs = ParseSimulationArgs(e.Args);

            // 建立 AppShell（單一 Window）
            var shell = new AppShell(
                _serviceProvider,
                _serviceProvider.GetRequiredService<SessionService>(),
                _serviceProvider.GetRequiredService<AuthService>(),
                _serviceProvider.GetRequiredService<TokenService>(),
                _serviceProvider.GetRequiredService<UvConfigService>(),
                _serviceProvider.GetRequiredService<IUvHardwareService>(),
                sysSettings);

            // 模擬器參數
            ApplySimArgs(shell, simArgs);

            // ESC / Alt+F4 關閉控制已由 AppShell 透過 SystemSettingService 統一管理

            shell.Show();
        }
        catch (Exception ex)
        {
            EventLogService.Instance?.LogException(
                "System", "App", ex, ErrorCodes.UnhandledException, "啟動失敗");

            MessageBox.Show(
                $"Error ID: {ErrorCodes.UnhandledException}\n\n" +
                $"{ex.Message}\n\n{ex.InnerException?.Message}",
                "TRIO2026 啟動錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // 記錄關閉事件並 flush 日誌
        EventLogService.Instance?.LogInfo("System", "App", ErrorCodes.AppShutdown, "應用程式關閉");

        if (EventLogService.Instance is IDisposable disposable)
            disposable.Dispose();

        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    /// <summary>
    /// 從 exe 位置向上尋找 Database/ 目錄所在的專案根目錄
    /// </summary>
    private static string FindProjectRoot()
    {
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        for (int i = 0; i < 8; i++)
        {
            if (Directory.Exists(Path.Combine(dir, "Database")))
                return dir;
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        // 回退: 使用 D:\TRIO2026（開發環境硬編碼）
        return @"D:\TRIO2026";
    }

    // ── 模擬器參數結構 ──
    private record SimArgs(int Width, int Height, bool Touch, bool Fullscreen, bool Embedded);

    private static SimArgs ParseSimulationArgs(string[] args)
    {
        int w = 0, h = 0;
        bool fs = false, touch = false, embed = false;
        for (int i = 0; i < args.Length - 1; i++)
        {
            switch (args[i])
            {
                case "--sim-width":    int.TryParse(args[i + 1], out w); break;
                case "--sim-height":   int.TryParse(args[i + 1], out h); break;
                case "--sim-touch":    touch = args[i + 1] == "1"; break;
                case "--sim-fullscreen": fs = args[i + 1] == "1"; break;
                case "--sim-embedded": embed = args[i + 1] == "1"; break;
            }
        }
        return new SimArgs(w, h, touch, fs, embed);
    }

    /// <summary>將模擬器參數套用到 AppShell</summary>
    private static void ApplySimArgs(Window window, SimArgs sim)
    {
        if (sim.Embedded)
        {
            window.WindowState = WindowState.Normal;
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            window.SizeToContent = SizeToContent.Manual;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = 0;
            window.Top = 0;
            if (sim.Width > 0) window.Width = sim.Width;
            if (sim.Height > 0) window.Height = sim.Height;
            window.Title = "TRIO2026";
        }
        else if (sim.Width > 0 && sim.Height > 0 && !sim.Fullscreen)
        {
            window.WindowState = WindowState.Normal;
            window.Width = sim.Width;
            window.Height = sim.Height;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Title = $"TRIO2026 — 模擬模式 ({sim.Width}×{sim.Height}{(sim.Touch ? " 觸控" : "")})";
        }
    }
}
