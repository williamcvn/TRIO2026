using System.Windows.Controls;
using TRIO2026.App.Controls;
using TRIO2026.App.Services;

namespace TRIO2026.App.Views.Pages;

/// <summary>
/// ServiceModePage — Service 角色專用頁面（placeholder，功能後續實作）
/// 
/// 導航來源：
///   - 免登入模式：UserMenu 的 Service Mode 按鈕 → LoginOverlay 驗證成功
///   - 登入模式：Service/Admin 角色登入後直接導向
/// 
/// 製作者: Office of William
/// </summary>
public partial class ServiceModePage : UserControl
{
    public ServiceModePage(SessionService sessionService,
        OverlayDialog dialogOverlay, LoginOverlay loginOverlay,
        AuthService authService, TokenService tokenService,
        SystemSettingService systemSettings)
    {
        InitializeComponent();

        // 初始化共用使用者選單
        UserMenu.Initialize(sessionService,
            dialogOverlay, loginOverlay, authService, tokenService, systemSettings);
    }

    /// <summary>供外部呼叫刷新使用者顯示</summary>
    public void RefreshUserDisplay()
    {
        UserMenu.RefreshUserDisplay();
    }
}
