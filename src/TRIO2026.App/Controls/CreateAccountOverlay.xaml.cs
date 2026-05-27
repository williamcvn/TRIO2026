using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TRIO2026.App.Services;
using TRIO2026.Core;

namespace TRIO2026.App.Controls;

/// <summary>
/// 新增帳號 Overlay — Admin 專用
/// 
/// 使用方式：
///   var result = await overlay.ShowAsync();
///   if (result.IsCreated) { ... result.TempPassword ... }
/// 
/// 流程：
///   1. 輸入 Username, DisplayName
///   2. 選擇角色（Operator / Admin，Service 不可透過 UI 建立）
///   3. 建立 → 顯示系統產生的臨時密碼（一次性）
/// 
/// 製作者: Office of William
/// </summary>
public partial class CreateAccountOverlay : UserControl
{
    private TaskCompletionSource<CreateAccountResult>? _tcs;
    private AccountManagementService? _accountService;
    private string? _operatorUsername;
    private int _selectedRole = 1; // 預設 Operator

    // 角色按鈕高亮色彩
    private static readonly SolidColorBrush SelectedBg = new(Color.FromRgb(0x1B, 0x8A, 0x6B));
    private static readonly SolidColorBrush SelectedBorder = new(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly SolidColorBrush NormalBg = new(Color.FromRgb(0x35, 0x4B, 0x70));
    private static readonly SolidColorBrush NormalBorder = new(Color.FromRgb(0x4A, 0x6A, 0x9A));

    public CreateAccountOverlay()
    {
        InitializeComponent();
    }

    /// <summary>初始化服務依賴</summary>
    public void Initialize(AccountManagementService accountService, string operatorUsername)
    {
        _accountService = accountService;
        _operatorUsername = operatorUsername;
    }

    /// <summary>顯示新增帳號 Overlay</summary>
    public Task<CreateAccountResult> ShowAsync()
    {
        var loc = LocalizationService.Instance;

        TitleText.Text = loc["AccountMgmt.CreateTitle"];
        UsernameLabel.Text = loc["AccountMgmt.LabelUsername"];
        DisplayNameLabel.Text = loc["AccountMgmt.LabelDisplayName"];
        RoleLabel.Text = loc["AccountMgmt.LabelRole"];
        ServiceNotice.Text = loc["AccountMgmt.ServiceNotice"];
        InitPasswordNotice.Text = loc["AccountMgmt.InitPasswordNotice"];
        CreateButton.Content = loc["AccountMgmt.CreateButton"];
        CancelButton.Content = loc["Common.Cancel"];

        UsernameBox.Text = "";
        DisplayNameBox.Text = "";
        _selectedRole = 1;
        ErrorText.Visibility = Visibility.Collapsed;

        _tcs = new TaskCompletionSource<CreateAccountResult>();
        Visibility = Visibility.Visible;

        // 進場動畫
        var scaleAnim = new DoubleAnimation(0.85, 1.0, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        CardScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, scaleAnim);
        CardScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, scaleAnim);
        var opacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
        BeginAnimation(OpacityProperty, opacityAnim);

        // 延遲到 Template Apply 後再更新角色按鈕外觀（確保 Operator 亮起）
        Dispatcher.BeginInvoke(() =>
        {
            // 強制 ApplyTemplate 以確保 FindName 可用
            BtnRoleOperator.ApplyTemplate();
            BtnRoleAdmin.ApplyTemplate();
            UpdateRoleButtons();
            DialogCard.Focus();
        });

        return _tcs.Task;
    }

    // ═══════════════════════════════════════
    // 角色選擇
    // ═══════════════════════════════════════

    private void OnRoleOperatorClick(object sender, RoutedEventArgs e)
    {
        _selectedRole = 1;
        UpdateRoleButtons();
    }

    private void OnRoleAdminClick(object sender, RoutedEventArgs e)
    {
        _selectedRole = 3;
        UpdateRoleButtons();
    }

    private void UpdateRoleButtons()
    {
        // 透過 Background / BorderBrush 動態更新按鈕外觀
        // 因為使用 ControlTemplate，需要直接操作 Template 內的 Border
        SetRoleButtonState(BtnRoleOperator, _selectedRole == 1);
        SetRoleButtonState(BtnRoleAdmin, _selectedRole == 3);
    }

    private static void SetRoleButtonState(Button btn, bool selected)
    {
        // 取得 ControlTemplate 中的 Border
        if (btn.Template?.FindName("Bd", btn) is Border bd)
        {
            bd.Background = selected ? SelectedBg : NormalBg;
            bd.BorderBrush = selected ? SelectedBorder : NormalBorder;
            bd.BorderThickness = selected ? new Thickness(2) : new Thickness(1);
        }
    }

    // ═══════════════════════════════════════
    // 建立 / 取消
    // ═══════════════════════════════════════

    private async void OnCreateClick(object sender, RoutedEventArgs e)
    {
        if (_accountService == null || _operatorUsername == null) return;

        var loc = LocalizationService.Instance;
        var username = UsernameBox.Text?.Trim() ?? "";
        var displayName = DisplayNameBox.Text?.Trim();
        if (string.IsNullOrEmpty(displayName)) displayName = null;

        var (success, error, tempPassword) = await _accountService.CreateUserAsync(
            username, displayName, _selectedRole, _operatorUsername);

        if (success)
        {
            EventLogService.Instance?.LogAccountMgmt(ErrorCodes.AccountCreated,
                "Account Created",
                $"Username={username}, Role={_selectedRole}, By={_operatorUsername}");
            Hide(new CreateAccountResult(false, true, tempPassword));
        }
        else
        {
            var msg = error switch
            {
                "INVALID_USERNAME" => loc["AccountMgmt.InvalidUsername"],
                "USERNAME_EXISTS" => loc["AccountMgmt.UsernameExists"],
                "INVALID_ROLE" => loc["AccountMgmt.ErrorServiceAdd"],
                _ => error ?? "Unknown error"
            };
            ErrorText.Text = msg;
            ErrorText.Visibility = Visibility.Visible;
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Hide(CreateAccountResult.Cancelled);
    }

    // ═══════════════════════════════════════
    // 觸控鍵盤
    // ═══════════════════════════════════════

    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        bool isUsername = textBox == UsernameBox;
        string initialText = textBox.Text ?? "";
        string titleKey = isUsername
            ? "TouchKeyboard.TitleAccount"
            : "AccountMgmt.LabelDisplayName";

        Dispatcher.BeginInvoke(new Action(() =>
        {
            DialogCard.Focus();
            TouchKeyboard.Show(false, initialText,
                result => { textBox.Text = result; },
                () => { });
        }));
    }

    private void Hide(CreateAccountResult result)
    {
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
        fadeOut.Completed += (s, e) =>
        {
            Visibility = Visibility.Collapsed;
            _tcs?.TrySetResult(result);
        };
        BeginAnimation(OpacityProperty, fadeOut);
    }
}

/// <summary>新增帳號結果</summary>
public record CreateAccountResult(bool IsCancelled, bool IsCreated = false, string? TempPassword = null)
{
    public static CreateAccountResult Cancelled => new(true);
}
