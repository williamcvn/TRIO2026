using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfComboBoxItem = System.Windows.Controls.ComboBoxItem;
using MessageBox = System.Windows.MessageBox;

namespace TRIO2026.DevLauncher;

public partial class LauncherWindow : Window
{
    // === Win32 Interop ===
    [DllImport("user32.dll")]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    // 取得螢幕真實物理尺寸
    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    private const int HORZSIZE = 4;    // 螢幕物理寬度 (mm)
    private const int VERTSIZE = 6;    // 螢幕物理高度 (mm)
    private const int HORZRES = 8;     // 螢幕像素寬度
    private const int VERTRES = 10;    // 螢幕像素高度

    private const int GWL_STYLE = -16;
    private const int WS_CHILD = 0x40000000;
    private const int SW_SHOW = 5;

    // 截圖用
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest,
        IntPtr hdcSrc, int xSrc, int ySrc, uint rop);

    private const uint SRCCOPY = 0x00CC0020;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    // === 狀態 ===
    private Process? _appProcess;
    private IntPtr _appWindowHandle = IntPtr.Zero;
    private int _simWidth = 600;
    private int _simHeight = 960;
    private double _scaleFactor = 1.0;
    private readonly DispatcherTimer _embedTimer = new();

    // 面板尺寸（公分），最小安全值
    private double _panelWidthCm = 9.5;    // 機台面板實際寬度
    private double _panelHeightCm = 15.0;   // 機台面板實際高度
    private const double MinPanelWidthCm = 9.0;
    private const double MinPanelHeightCm = 9.0;

    // 螢幕真實物理像素密度（pixels per cm）
    private double _realPixelsPerCmX = 96.0 / 2.54;  // 預設 96 DPI
    private double _realPixelsPerCmY = 96.0 / 2.54;
    private double _wpfScaleX = 1.0; // WPF DPI 縮放

    // 工具列 + 狀態列高度估算
    private const double ToolbarHeight = 90;  // 兩行工具列
    private const double StatusBarHeight = 30;
    private const double ChromeHeight = 38; // 視窗標題列

    // Win32 嵌入容器
    private readonly System.Windows.Forms.Panel _hostPanel = new()
    {
        Dock = System.Windows.Forms.DockStyle.Fill,
        BackColor = System.Drawing.Color.FromArgb(17, 24, 39) // #111827
    };

    public LauncherWindow()
    {
        InitializeComponent();

        // 將 WinForms Panel 放入 WindowsFormsHost
        WfHost.Child = _hostPanel;

        _embedTimer.Interval = TimeSpan.FromMilliseconds(50);  // 縮短輪詢間隔，加速嵌入
        _embedTimer.Tick += EmbedTimer_Tick;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // 偵測螢幕真實物理尺寸
        DetectRealScreenSize();

        // 套用初始解析度
        ApplyResolution();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        KillAppProcess();
    }

    /// <summary>解析度下拉選單變更</summary>
    private void Resolution_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // XAML 初始化時 ComboBox 會觸發此事件，但 UI 元素尚未建立
        if (!IsLoaded) return;

        if (ResolutionCombo.SelectedItem is System.Windows.Controls.ComboBoxItem item && item.Tag is string tag)
        {
            var parts = tag.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0], out var w) && int.TryParse(parts[1], out var h))
            {
                _simWidth = w;
                _simHeight = h;
                ApplyResolution();
            }
        }
    }

    /// <summary>根據選擇的解析度計算模擬器容器大小</summary>
    private void ApplyResolution()
    {
        // 直接用選擇的解析度作為 WPF 邏輯像素（= 機台上 App 看到的像素數）
        var panelWpfW = (double)_simWidth;
        var panelWpfH = (double)_simHeight;

        // 計算可用的最大螢幕空間（扣除 Taskbar 安全邊距）
        var maxW = SystemParameters.WorkArea.Width - 40;
        var maxH = SystemParameters.WorkArea.Height - 40;
        var contentMaxW = maxW;
        var contentMaxH = maxH - ToolbarHeight - StatusBarHeight - ChromeHeight;

        // 若解析度超過開發機螢幕，等比縮小
        _scaleFactor = Math.Min(
            contentMaxW / panelWpfW,
            contentMaxH / panelWpfH);
        if (_scaleFactor > 1.0) _scaleFactor = 1.0;

        var displayW = panelWpfW * _scaleFactor;
        var displayH = panelWpfH * _scaleFactor;

        // 設定螢幕容器大小
        ScreenContainer.Width = displayW;
        ScreenContainer.Height = displayH;

        // 更新 UI 顯示
        var scaleInfo = _scaleFactor < 1.0 ? $" (⚠ 超出螢幕，縮放 {_scaleFactor * 100:F0}%)" : "";
        SimResText.Text = $"解析度: {_simWidth}×{_simHeight} | 面板: {_panelWidthCm:F1}×{_panelHeightCm:F1} cm{scaleInfo}";
        ScaleText.Text = $"{_scaleFactor * 100:F0}%";

        // 調整視窗大小
        SizeToContent = SizeToContent.Manual;
        Width = displayW + 24;
        Height = displayH + ToolbarHeight + StatusBarHeight + ChromeHeight + 12;

        // 重新置中
        Left = (SystemParameters.WorkArea.Width - Width) / 2;
        Top = (SystemParameters.WorkArea.Height - Height) / 2;

        // 如果 App 已嵌入，重新調整子視窗大小
        if (_appWindowHandle != IntPtr.Zero)
        {
            MoveWindow(_appWindowHandle, 0, 0, (int)displayW, (int)displayH, true);
        }

        StatusText.Text = $"模擬: {_simWidth}×{_simHeight} | 面板: {_panelWidthCm:F1}×{_panelHeightCm:F1} cm | 真實 PPI: {_realPixelsPerCmX * 2.54:F1} | 縮放: {_scaleFactor * 100:F0}%";
    }

    /// <summary>偵測螢幕真實物理尺寸，計算真實 pixels-per-cm</summary>
    private void DetectRealScreenSize()
    {
        try
        {
            var hdc = GetDC(IntPtr.Zero);
            var physWidthMm = GetDeviceCaps(hdc, HORZSIZE);   // 物理寬 (mm)
            var physHeightMm = GetDeviceCaps(hdc, VERTSIZE);  // 物理高 (mm)
            var pixelWidth = GetDeviceCaps(hdc, HORZRES);     // 像素寬
            var pixelHeight = GetDeviceCaps(hdc, VERTRES);    // 像素高
            ReleaseDC(IntPtr.Zero, hdc);

            if (physWidthMm > 0 && physHeightMm > 0)
            {
                // 真實 pixels per cm
                _realPixelsPerCmX = pixelWidth / (physWidthMm / 10.0);
                _realPixelsPerCmY = pixelHeight / (physHeightMm / 10.0);
            }

            // WPF DPI 縮放
            var src = PresentationSource.FromVisual(this);
            if (src?.CompositionTarget != null)
                _wpfScaleX = src.CompositionTarget.TransformToDevice.M11;

            var physWcm = physWidthMm / 10.0;
            var physHcm = physHeightMm / 10.0;
            var diagInch = Math.Sqrt(physWcm * physWcm + physHcm * physHcm) / 2.54;
            var hasTouch = Tablet.TabletDevices.Count > 0;

            DevScreenInfo.Text = $"開發機: {pixelWidth}×{pixelHeight} | {physWcm:F1}×{physHcm:F1} cm ({diagInch:F1}\") | PPI: {_realPixelsPerCmX * 2.54:F0} | 觸控: {(hasTouch ? "✅" : "❌")}";
        }
        catch
        {
            DevScreenInfo.Text = "開發機: 無法偵測螢幕資訊";
        }
    }

    /// <summary>面板尺寸文字變更（即時驗證）</summary>
    private void PanelSize_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // 僅做即時文字顏色回饋，不自動套用
        if (!IsLoaded) return;

        if (sender is WpfTextBox tb)
        {
            if (double.TryParse(tb.Text, out _))
                tb.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Colors.White);
            else
                tb.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF5350"));
        }
    }

    /// <summary>套用按鈕：讀取面板尺寸並更新模擬器</summary>
    private void ApplySize_Click(object sender, RoutedEventArgs e)
    {
        if (!double.TryParse(PanelWidthBox.Text, out var w) || !double.TryParse(PanelHeightBox.Text, out var h))
        {
            MessageBox.Show("請輸入有效的數值（例: 52.0）", "輸入錯誤", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 強制最小安全值
        if (w < MinPanelWidthCm) { w = MinPanelWidthCm; PanelWidthBox.Text = w.ToString("F1"); }
        if (h < MinPanelHeightCm) { h = MinPanelHeightCm; PanelHeightBox.Text = h.ToString("F1"); }

        _panelWidthCm = w;
        _panelHeightCm = h;
        ApplyResolution();
    }

    /// <summary>截圖整個模擬器視窗（使用 Win32 擷取，包含嵌入子視窗）</summary>
    private void Screenshot_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dir = @"D:\TRIO2026\tools\Screenshots";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // 取得視窗在螢幕上的位置與大小
            var helper = new WindowInteropHelper(this);
            GetWindowRect(helper.Handle, out var rect);
            var w = rect.Right - rect.Left;
            var h = rect.Bottom - rect.Top;

            // 用 BitBlt 從螢幕擷取（包含嵌入的子視窗）
            var hdcScreen = GetDC(IntPtr.Zero);
            var hdcMem = CreateCompatibleDC(hdcScreen);
            var hBitmap = CreateCompatibleBitmap(hdcScreen, w, h);
            var hOld = SelectObject(hdcMem, hBitmap);

            BitBlt(hdcMem, 0, 0, w, h, hdcScreen, rect.Left, rect.Top, SRCCOPY);

            SelectObject(hdcMem, hOld);

            // 轉為 BitmapSource 並儲存
            var bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmpSrc));

            var filename = $"sim_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var filepath = Path.Combine(dir, filename);
            using (var fs = new FileStream(filepath, FileMode.Create))
                encoder.Save(fs);

            // 清理
            DeleteObject(hBitmap);
            DeleteDC(hdcMem);
            ReleaseDC(IntPtr.Zero, hdcScreen);

            StatusText.Text = $"📷 截圖已儲存: {filename}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"截圖失敗: {ex.Message}";
        }
    }

    /// <summary>啟動 TRIO2026.App</summary>
    private void Run_Click(object sender, RoutedEventArgs e)
    {
        var appExe = FindAppExe();
        if (appExe == null)
        {
            MessageBox.Show(
                "找不到 TRIO2026.App.exe\n\n請先在 VS 中建置 TRIO2026.App 專案。",
                "找不到應用程式", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            // 切換顯示：隱藏佔位文字，顯示嵌入容器
            PlaceholderGrid.Visibility = Visibility.Collapsed;
            WfHost.Visibility = Visibility.Visible;

            // 使用 ScreenContainer 的 WPF 邏輯像素尺寸（已完成佈局，數值可靠）
            var logicalW = (int)ScreenContainer.ActualWidth;
            var logicalH = (int)ScreenContainer.ActualHeight;

            // 啟動 App（傳入 WPF 邏輯像素尺寸 — App 用這個設定 Window.Width/Height）
            var psi = new ProcessStartInfo
            {
                FileName = appExe,
                Arguments = $"--sim-width {logicalW} --sim-height {logicalH} --sim-touch 1 --sim-fullscreen 0 --sim-embedded 1",
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(appExe)!
            };

            _appProcess = Process.Start(psi);

            // 啟動定時器等待視窗出現後嵌入
            _embedTimer.Start();

            RunButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            ResolutionCombo.IsEnabled = false;
            StatusText.Text = "正在啟動 TRIO2026.App ...";
        }
        catch (Exception ex)
        {
            PlaceholderGrid.Visibility = Visibility.Visible;
            WfHost.Visibility = Visibility.Collapsed;
            MessageBox.Show($"啟動失敗:\n{ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>定時檢查 App 視窗是否出現，出現後嵌入</summary>
    private void EmbedTimer_Tick(object? sender, EventArgs e)
    {
        if (_appProcess == null || _appProcess.HasExited)
        {
            _embedTimer.Stop();
            OnAppExited();
            return;
        }

        // 嘗試取得主視窗 Handle
        _appProcess.Refresh();
        var hWnd = _appProcess.MainWindowHandle;

        if (hWnd == IntPtr.Zero) return;  // 視窗還沒出現，繼續等

        _embedTimer.Stop();
        _appWindowHandle = hWnd;

        // 使用 WinForms Panel 的 Handle 作為父視窗（限制在模擬區域內）
        var containerHwnd = _hostPanel.Handle;

        // 移除 App 視窗的標題列和邊框
        var style = GetWindowLong(hWnd, GWL_STYLE);
        SetWindowLong(hWnd, GWL_STYLE, (style & ~0x00C00000 & ~0x00040000) | WS_CHILD);

        // 設定父視窗為 Panel（不是整個視窗）
        SetParent(hWnd, containerHwnd);

        // 調整大小填滿 Panel（MoveWindow 使用裝置像素）
        var displayW = (int)(ScreenContainer.ActualWidth * _wpfScaleX);
        var displayH = (int)(ScreenContainer.ActualHeight * _wpfScaleX);
        MoveWindow(hWnd, 0, 0, displayW, displayH, true);
        ShowWindow(hWnd, SW_SHOW);

        StatusText.Text = $"✅ TRIO2026.App 運行中 — {(int)ScreenContainer.ActualWidth}×{(int)ScreenContainer.ActualHeight} (面板 {_panelWidthCm}×{_panelHeightCm} cm)";

        // 監控 App 是否退出
        _appProcess.EnableRaisingEvents = true;
        _appProcess.Exited += (s, args) => Dispatcher.Invoke(OnAppExited);
    }

    /// <summary>停止 App</summary>
    private void Stop_Click(object sender, RoutedEventArgs e)
    {
        KillAppProcess();
    }

    private void KillAppProcess()
    {
        _embedTimer.Stop();
        if (_appProcess != null && !_appProcess.HasExited)
        {
            try { _appProcess.Kill(); } catch { }
        }
        _appProcess = null;
        _appWindowHandle = IntPtr.Zero;
        OnAppExited();
    }

    private void OnAppExited()
    {
        PlaceholderGrid.Visibility = Visibility.Visible;
        WfHost.Visibility = Visibility.Collapsed;
        RunButton.IsEnabled = true;
        StopButton.IsEnabled = false;
        ResolutionCombo.IsEnabled = true;
        StatusText.Text = "已停止";
    }

    /// <summary>搜尋 TRIO2026.App.exe 位置</summary>
    private static string? FindAppExe()
    {
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        for (int i = 0; i < 8; i++)
        {
            var candidates = new[]
            {
                Path.Combine(dir, "App", "TRIO2026.App.exe"), // Demo package path
                Path.Combine(dir, "src", "TRIO2026.App", "bin", "Debug", "net8.0-windows", "TRIO2026.App.exe"),
                Path.Combine(dir, "src", "TRIO2026.App", "bin", "Release", "net8.0-windows", "TRIO2026.App.exe"),
            };
            foreach (var c in candidates)
                if (File.Exists(c)) return c;

            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        return null;
    }
}
