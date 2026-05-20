using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TRIO2026.App.Controls;
using TRIO2026.App.Services;
using TRIO2026.App.Views.Pages;
using TRIO2026.Core;
using TRIO2026.Core.Entities;
using TRIO2026.Core.Enums;
using TRIO2026.Core.Interfaces;
using TRIO2026.Data.Contexts;

namespace TRIO2026.App.Views;

/// <summary>
/// 應用程式主殼層 — 單一 Window，所有頁面在此切換
/// 對應舊系統 MainWindow + widgetmap 的架構
/// </summary>
public partial class AppShell : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SessionService _sessionService;
    private readonly AuthService _authService;
    private readonly TokenService _tokenService;
    private readonly UvConfigService _uvConfigService;
    private readonly IUvHardwareService _uvHardwareService;
    private readonly SystemSettingService _systemSettings;

    // 頁面實例（預先建立，hide/show 切換）
    private LoginPage? _loginPage;
    private InitPage? _initPage;
    private MenuPage? _menuPage;
    private UvDecontaminationPage? _uvPage;
    private ServiceModePage? _serviceModePage;

    public AppShell(IServiceProvider serviceProvider,
        SessionService sessionService,
        AuthService authService, TokenService tokenService,
        UvConfigService uvConfigService, IUvHardwareService uvHardwareService,
        SystemSettingService systemSettings)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _sessionService = sessionService;
        _authService = authService;
        _tokenService = tokenService;
        _uvConfigService = uvConfigService;
        _uvHardwareService = uvHardwareService;
        _systemSettings = systemSettings;

        // 視窗關閉事件 — 根據 DB 設定決定是否允許
        Closing += OnWindowClosing;

        // ESC 鍵 — 根據 DB 設定決定是否允許關閉
        PreviewKeyDown += (s, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                if (_systemSettings.EscKeyCloseEnabled)
                {
                    EventLogService.Instance?.LogInfo("System", "AppShell", ErrorCodes.AppShutdown,
                        "ESC 鍵關閉");
                    Application.Current.Shutdown();
                }
                else
                {
                    e.Handled = true; // DB 設定為停用 → 攔截
                }
            }
        };
    }

    private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Alt+F4 觸發 → 檢查 DB 設定
        if (!_systemSettings.AltF4CloseEnabled)
        {
            e.Cancel = true; // DB 設定為停用 → 取消關閉
            return;
        }

        // 允許關閉 → 記錄日誌
        EventLogService.Instance?.LogInfo("System", "AppShell", ErrorCodes.AppShutdown,
            "視窗關閉請求（Alt+F4）");
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        // 根據設定決定起始頁面（讀取 system_config.db）
        if (_systemSettings.LoginRequired)
            NavigateTo("login");
        else
            NavigateTo("init");
    }

    // ═══════ 頁面切換（對應舊系統 slotswitchpage） ═══════

    private string? _currentPage;

    public void NavigateTo(string page)
    {
        var fromPage = _currentPage;
        _currentPage = page;

        // 操作追蹤：頁面導航
        EventLogService.Instance.LogNavigation(fromPage, page);

        switch (page)
        {
            case "login":
                _loginPage ??= CreateLoginPage();
                PageHost.Content = _loginPage;
                break;

            case "init":
                _initPage ??= CreateInitPage();
                PageHost.Content = _initPage;
                break;

            case "menu":
                _menuPage ??= CreateMenuPage();
                _menuPage.RefreshUserDisplay();
                PageHost.Content = _menuPage;
                break;

            case "uv":
                _uvPage ??= CreateUvPage();
                PageHost.Content = _uvPage;
                _uvPage.OnNavigatedTo(fromPage);
                break;

            case "service":
                _serviceModePage ??= CreateServiceModePage();
                _serviceModePage.RefreshUserDisplay();
                PageHost.Content = _serviceModePage;
                break;
        }
    }

    // ═══════ 頁面工廠 ═══════

    private LoginPage CreateLoginPage()
    {
        var vm = new ViewModels.LoginViewModel(_authService, _sessionService, _tokenService);
        var page = new LoginPage(vm, _systemSettings);
        page.LoginSucceeded += OnLoginSucceeded;
        page.CloseRequested += OnCloseRequested;
        return page;
    }

    private InitPage CreateInitPage()
    {
        var page = new InitPage(_systemSettings);
        page.CountdownCompleted += OnInitCompleted;
        return page;
    }

    private MenuPage CreateMenuPage()
    {
        return new MenuPage(_sessionService, DialogOverlay, LoginOverlayHost,
            _authService, _tokenService, _systemSettings);
    }

    private UvDecontaminationPage CreateUvPage()
    {
        var vm = new ViewModels.UvDecontaminationViewModel(_uvConfigService, _uvHardwareService);
        return new UvDecontaminationPage(vm, _sessionService,
            DialogOverlay, LoginOverlayHost, _authService, _tokenService, _systemSettings);
    }

    private ServiceModePage CreateServiceModePage()
    {
        return new ServiceModePage(_sessionService, DialogOverlay, LoginOverlayHost,
            _authService, _tokenService, _systemSettings);
    }

    // ═══════ 頁面事件處理 ═══════

    private void OnLoginSucceeded(object? sender, EventArgs e)
    {
        var role = _sessionService.CurrentRole;
        if (role == RoleLevel.Service)
        {
            // Service 登入 → ServiceModePage
            _serviceModePage = null; // 重新建立以刷新使用者資訊
            NavigateTo("service");
        }
        else
        {
            // Operator / Admin 登入 → MenuPage
            _menuPage = null; // 重新建立以刷新使用者資訊
            NavigateTo("menu");
        }
    }

    private void OnInitCompleted(object? sender, EventArgs e)
    {
        // Init 倒數結束 → 從 DB 載入免登入帳號 → 進入選單
        var guestUser = LoadGuestUser();
        _sessionService.SetGuestSession(guestUser, _systemSettings.GuestAccountDisplayName);
        _menuPage = null;
        NavigateTo("menu");
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Application.Current.Shutdown();
    }

    // ═══════ Guest User 載入 ═══════

    /// <summary>從 main.db 載入免登入專用帳號</summary>
    private User LoadGuestUser()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppMainDbContext>();
            var username = _systemSettings.GuestAccountUsername;
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user != null) return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AppShell] LoadGuestUser failed: {ex.Message}");
        }

        // Fallback: DB 中無此帳號，建構最小化物件
        return new User
        {
            Id = 0,
            Username = _systemSettings.GuestAccountUsername,
            DisplayName = _systemSettings.GuestAccountDisplayName,
            RoleLevel = (int)RoleLevel.Operator,
            IsActive = 1,
            PasswordHash = "",
            CreatedAt = DateTime.UtcNow.ToString("o"),
            CreatedBy = "SYSTEM"
        };
    }

    /// <summary>恢復免登入模式的 Guest Session（Service Mode 退出時使用）</summary>
    public void RestoreGuestSession()
    {
        var guestUser = LoadGuestUser();
        _sessionService.SetGuestSession(guestUser, _systemSettings.GuestAccountDisplayName);
        _menuPage = null; // 重新建立以刷新使用者資訊
    }
}
