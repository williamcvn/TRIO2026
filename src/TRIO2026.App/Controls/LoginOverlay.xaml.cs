using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace TRIO2026.App.Controls;

/// <summary>
/// 身分驗證 Overlay — 在應用程式內部顯示帳號密碼輸入
/// 驗證成功後回傳已認證使用者的 RoleLevel
/// </summary>
public partial class LoginOverlay : UserControl
{
    private TaskCompletionSource<LoginOverlayResult>? _tcs;

    public LoginOverlay()
    {
        InitializeComponent();
    }

    /// <summary>顯示身分驗證 Overlay</summary>
    public Task<LoginOverlayResult> ShowAsync(string title = "身分驗證",
        string subtitle = "請輸入帳號密碼以進行身分確認")
    {
        TitleText.Text = title;
        SubtitleText.Text = subtitle;
        UsernameBox.Text = "";
        PasswordBox.Password = "";
        ErrorText.Visibility = Visibility.Collapsed;
        ErrorText.Text = "";

        _tcs = new TaskCompletionSource<LoginOverlayResult>();
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

        // 自動聚焦帳號輸入框
        Dispatcher.BeginInvoke(() => UsernameBox.Focus());

        return _tcs.Task;
    }

    /// <summary>顯示驗證錯誤訊息</summary>
    public void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
        PasswordBox.Password = "";
        PasswordBox.Focus();
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        var username = UsernameBox.Text?.Trim() ?? "";
        var password = PasswordBox.Password ?? "";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("請輸入帳號和密碼");
            return;
        }

        // 回傳 credentials，由呼叫端驗證
        Hide(new LoginOverlayResult(false, username, password));
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Hide(LoginOverlayResult.Cancelled);
    }

    private void Hide(LoginOverlayResult result)
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

/// <summary>LoginOverlay 回傳結果</summary>
public record LoginOverlayResult(bool IsCancelled, string Username = "", string Password = "")
{
    public static LoginOverlayResult Cancelled => new(true);
}
