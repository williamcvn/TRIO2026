using System.Windows;
using System.Windows.Controls;
using TRIO2026.App.Controls;
using TRIO2026.App.Services;
using TRIO2026.App.Views;

namespace TRIO2026.App.Views.Pages;

/// <summary>
/// MenuPage — 02 主選單頁面，5 個功能按鈕
/// 使用者選單由共用 UserMenuControl 處理
/// </summary>
public partial class MenuPage : UserControl
{
    private readonly SessionService _sessionService;
    private readonly OverlayDialog _dialogOverlay;

    public MenuPage(SessionService sessionService,
        OverlayDialog dialogOverlay, LoginOverlay loginOverlay,
        AuthService authService, TokenService tokenService,
        SystemSettingService systemSettings)
    {
        InitializeComponent();
        _sessionService = sessionService;
        _dialogOverlay = dialogOverlay;

        // 初始化共用使用者選單
        UserMenu.Initialize(sessionService,
            dialogOverlay, loginOverlay, authService, tokenService, systemSettings);
    }

    /// <summary>供外部呼叫刷新使用者顯示</summary>
    public void RefreshUserDisplay()
    {
        UserMenu.RefreshUserDisplay();
    }

    // ── 功能按鈕 ──

    private async void OnIntelliPlexClick(object sender, RoutedEventArgs e)
    {
        EventLogService.Instance.LogButtonClick("MenuPage", "IntelliPlex");
        await _dialogOverlay.ShowAsync("IntelliPlex Program", "功能開發中...", "確定", OverlayDialogIcon.Info);
    }

    private async void OnCustomClick(object sender, RoutedEventArgs e)
    {
        EventLogService.Instance.LogButtonClick("MenuPage", "Custom");
        await _dialogOverlay.ShowAsync("Custom Program", "功能開發中...", "確定", OverlayDialogIcon.Info);
    }

    private async void OnDataClick(object sender, RoutedEventArgs e)
    {
        EventLogService.Instance.LogButtonClick("MenuPage", "Data");
        await _dialogOverlay.ShowAsync("Data", "功能開發中...", "確定", OverlayDialogIcon.Info);
    }

    private async void OnSettingClick(object sender, RoutedEventArgs e)
    {
        EventLogService.Instance.LogButtonClick("MenuPage", "Setting");
        await _dialogOverlay.ShowAsync("Setting", "功能開發中...", "確定", OverlayDialogIcon.Info);
    }

    private void OnUVClick(object sender, RoutedEventArgs e)
    {
        EventLogService.Instance.LogButtonClick("MenuPage", "UV");
        var shell = Window.GetWindow(this) as AppShell;
        shell?.NavigateTo("uv");
    }
}
