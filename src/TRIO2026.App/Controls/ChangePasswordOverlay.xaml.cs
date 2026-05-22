using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TRIO2026.App.Services;

namespace TRIO2026.App.Controls;

/// <summary>
/// 密碼變更 Overlay — 含即時密碼原則驗證提示
/// 
/// 使用方式：
///   var result = await overlay.ShowAsync(userId, roleLevel, isForced);
///   // result.IsSuccess → 密碼已成功變更
///   // result.IsCancelled → 使用者取消
/// 
/// 規格：
///   - 三欄位：當前密碼、新密碼、確認密碼
///   - 各欄位含 👁 明碼切換按鈕
///   - 新密碼輸入時即時顯示原則驗證狀態（✅/❌ 逐條）
///   - ForcePasswordChange 模式下隱藏取消按鈕
/// 
/// 製作者: Office of William
/// </summary>
public partial class ChangePasswordOverlay : UserControl
{
    private TaskCompletionSource<ChangePasswordResult>? _tcs;
    private AuthService? _authService;
    private PasswordPolicyService? _policyService;
    private int _userId;
    private int _roleLevel;
    private bool _isForced;
    private bool _suppressSync; // 防止明碼/密碼欄位同步遞迴

    public ChangePasswordOverlay()
    {
        InitializeComponent();
    }

    /// <summary>初始化服務依賴</summary>
    public void Initialize(AuthService authService, PasswordPolicyService policyService)
    {
        _authService = authService;
        _policyService = policyService;
    }

    /// <summary>顯示密碼變更 Overlay</summary>
    public Task<ChangePasswordResult> ShowAsync(int userId, int roleLevel, bool isForced = false)
    {
        _userId = userId;
        _roleLevel = roleLevel;
        _isForced = isForced;

        var loc = LocalizationService.Instance;

        // 設定標題、說明文字
        TitleText.Text = loc["PasswordUI.Title"];
        SubtitleText.Text = isForced ? loc["PasswordUI.ForceSubtitle"] : loc["PasswordUI.Subtitle"];
        CurrentPasswordLabel.Text = loc["PasswordUI.CurrentPassword"];
        NewPasswordLabel.Text = loc["PasswordUI.NewPassword"];
        ConfirmPasswordLabel.Text = loc["PasswordUI.ConfirmPassword"];
        SubmitButton.Content = loc["PasswordUI.Submit"];
        CancelButton.Content = loc["Common.Cancel"];

        // ForcePasswordChange 模式隱藏取消按鈕
        CancelButton.Visibility = isForced ? Visibility.Collapsed : Visibility.Visible;

        // 清空欄位
        ClearFields();

        // 初始化規則提示
        RefreshPolicyRules("");

        _tcs = new TaskCompletionSource<ChangePasswordResult>();
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

        Dispatcher.BeginInvoke(() => CurrentPasswordBox.Focus());

        return _tcs.Task;
    }

    // ═══════════════════════════════════════
    // 密碼顯示/隱藏切換
    // ═══════════════════════════════════════

    private bool _currentVisible;
    private bool _newVisible;
    private bool _confirmVisible;

    private void OnToggleCurrentPassword(object sender, RoutedEventArgs e)
    {
        _currentVisible = !_currentVisible;
        TogglePasswordVisibility(CurrentPasswordBox, CurrentPasswordPlain, _currentVisible);
    }

    private void OnToggleNewPassword(object sender, RoutedEventArgs e)
    {
        _newVisible = !_newVisible;
        TogglePasswordVisibility(NewPasswordBox, NewPasswordPlain, _newVisible);
    }

    private void OnToggleConfirmPassword(object sender, RoutedEventArgs e)
    {
        _confirmVisible = !_confirmVisible;
        TogglePasswordVisibility(ConfirmPasswordBox, ConfirmPasswordPlain, _confirmVisible);
    }

    private void TogglePasswordVisibility(PasswordBox pwBox, TextBox plainBox, bool showPlain)
    {
        _suppressSync = true;
        if (showPlain)
        {
            plainBox.Text = pwBox.Password;
            pwBox.Visibility = Visibility.Collapsed;
            plainBox.Visibility = Visibility.Visible;
            plainBox.Focus();
            plainBox.CaretIndex = plainBox.Text.Length;
        }
        else
        {
            pwBox.Password = plainBox.Text;
            plainBox.Visibility = Visibility.Collapsed;
            pwBox.Visibility = Visibility.Visible;
            pwBox.Focus();
        }
        _suppressSync = false;
    }

    // ═══════════════════════════════════════
    // 即時驗證
    // ═══════════════════════════════════════

    private void OnPasswordFieldChanged(object sender, RoutedEventArgs e)
    {
        if (_suppressSync) return;
        var newPw = GetNewPassword();
        RefreshPolicyRules(newPw);
    }

    private void OnPlainTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressSync) return;

        // 同步明碼回密碼欄位
        _suppressSync = true;
        if (sender == CurrentPasswordPlain && _currentVisible)
            CurrentPasswordBox.Password = CurrentPasswordPlain.Text;
        else if (sender == NewPasswordPlain && _newVisible)
            NewPasswordBox.Password = NewPasswordPlain.Text;
        else if (sender == ConfirmPasswordPlain && _confirmVisible)
            ConfirmPasswordBox.Password = ConfirmPasswordPlain.Text;
        _suppressSync = false;

        var newPw = GetNewPassword();
        RefreshPolicyRules(newPw);
    }

    private void RefreshPolicyRules(string newPassword)
    {
        if (_policyService == null) return;

        var rules = _policyService.GetPolicyRules(newPassword, _roleLevel);
        var displayRules = rules.Select(r => new PolicyRuleDisplay
        {
            Icon = r.IsMet ? "✅" : "❌",
            Description = r.Description,
            Color = r.IsMet
                ? new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50))
                : new SolidColorBrush(Color.FromRgb(0x90, 0xA4, 0xAE))
        }).ToList();

        PolicyRulesPanel.ItemsSource = displayRules;
    }

    // ═══════════════════════════════════════
    // 提交 / 取消
    // ═══════════════════════════════════════

    private async void OnSubmitClick(object sender, RoutedEventArgs e)
    {
        if (_authService == null) return;

        var loc = LocalizationService.Instance;
        var currentPw = GetCurrentPassword();
        var newPw = GetNewPassword();
        var confirmPw = GetConfirmPassword();

        // 前端驗證：確認密碼
        if (newPw != confirmPw)
        {
            ShowError(loc["PasswordUI.PasswordMismatch"]);
            return;
        }

        // 前端驗證：新舊不同
        if (currentPw == newPw)
        {
            ShowError(loc["PasswordUI.SamePassword"]);
            return;
        }

        // 呼叫 AuthService.ChangePasswordAsync
        var (success, error) = await _authService.ChangePasswordAsync(_userId, currentPw, newPw);

        if (success)
        {
            EventLogService.Instance?.LogPasswordChange(true, _userId);
            Hide(new ChangePasswordResult(false, true));
        }
        else
        {
            // 對應錯誤碼的多語系訊息
            var msg = error switch
            {
                "WRONG_CURRENT_PASSWORD" => loc["PasswordUI.WrongCurrentPassword"],
                "SAME_PASSWORD" => loc["PasswordUI.SamePassword"],
                _ => error ?? "Unknown error"
            };
            ShowError(msg);

            EventLogService.Instance?.LogPasswordChange(false, _userId, error);
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Hide(ChangePasswordResult.Cancelled);
    }

    // ═══════════════════════════════════════
    // 輔助方法
    // ═══════════════════════════════════════

    private string GetCurrentPassword() =>
        _currentVisible ? CurrentPasswordPlain.Text : CurrentPasswordBox.Password;

    private string GetNewPassword() =>
        _newVisible ? NewPasswordPlain.Text : NewPasswordBox.Password;

    private string GetConfirmPassword() =>
        _confirmVisible ? ConfirmPasswordPlain.Text : ConfirmPasswordBox.Password;

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private void ClearFields()
    {
        _suppressSync = true;
        CurrentPasswordBox.Password = "";
        NewPasswordBox.Password = "";
        ConfirmPasswordBox.Password = "";
        CurrentPasswordPlain.Text = "";
        NewPasswordPlain.Text = "";
        ConfirmPasswordPlain.Text = "";
        ErrorText.Text = "";
        ErrorText.Visibility = Visibility.Collapsed;
        _currentVisible = false;
        _newVisible = false;
        _confirmVisible = false;
        CurrentPasswordPlain.Visibility = Visibility.Collapsed;
        NewPasswordPlain.Visibility = Visibility.Collapsed;
        ConfirmPasswordPlain.Visibility = Visibility.Collapsed;
        CurrentPasswordBox.Visibility = Visibility.Visible;
        NewPasswordBox.Visibility = Visibility.Visible;
        ConfirmPasswordBox.Visibility = Visibility.Visible;
        _suppressSync = false;
    }

    private void Hide(ChangePasswordResult result)
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

/// <summary>密碼變更結果</summary>
public record ChangePasswordResult(bool IsCancelled, bool IsSuccess = false)
{
    public static ChangePasswordResult Cancelled => new(true);
}

/// <summary>密碼規則 UI 顯示模型</summary>
public class PolicyRuleDisplay
{
    public string Icon { get; set; } = "";
    public string Description { get; set; } = "";
    public Brush Color { get; set; } = Brushes.Gray;
}
