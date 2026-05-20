using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TRIO2026.App.Controls;

/// <summary>
/// 自訂 WPF Overlay Dialog — 取代 OS MessageBox
/// 支援單按鈕、雙按鈕、三按鈕模式
/// </summary>
public partial class OverlayDialog : UserControl
{
    private TaskCompletionSource<bool>? _tcs;
    private TaskCompletionSource<int>? _tcsInt;

    public OverlayDialog()
    {
        InitializeComponent();
    }

    /// <summary>顯示訊息對話框（單按鈕）</summary>
    public Task<bool> ShowAsync(string title, string message,
        string buttonText = "確定", OverlayDialogIcon icon = OverlayDialogIcon.Info)
    {
        TitleText.Text = title;
        MessageText.Text = message;
        PrimaryButton.Content = buttonText;
        SecondaryButton.Visibility = Visibility.Collapsed;
        MiddleButton.Visibility = Visibility.Collapsed;
        IconText.Text = GetIconText(icon);
        IconBorder.Background = new SolidColorBrush(GetIconBackground(icon));

        return Show();
    }

    /// <summary>顯示確認對話框（雙按鈕）</summary>
    public Task<bool> ShowConfirmAsync(string title, string message,
        string confirmText = "確定", string cancelText = "取消",
        OverlayDialogIcon icon = OverlayDialogIcon.Warning)
    {
        TitleText.Text = title;
        MessageText.Text = message;
        PrimaryButton.Content = confirmText;
        SecondaryButton.Content = cancelText;
        SecondaryButton.Visibility = Visibility.Visible;
        MiddleButton.Visibility = Visibility.Collapsed;
        IconText.Text = GetIconText(icon);
        IconBorder.Background = new SolidColorBrush(GetIconBackground(icon));

        return Show();
    }

    /// <summary>顯示三選一對話框（三按鈕）</summary>
    /// <returns>0=主要, 1=中間, 2=取消</returns>
    public Task<int> ShowTripleAsync(string title, string message,
        string primaryText, string middleText, string cancelText,
        OverlayDialogIcon icon = OverlayDialogIcon.Warning)
    {
        TitleText.Text = title;
        MessageText.Text = message;
        PrimaryButton.Content = primaryText;
        MiddleButton.Content = middleText;
        MiddleButton.Visibility = Visibility.Visible;
        SecondaryButton.Content = cancelText;
        SecondaryButton.Visibility = Visibility.Visible;
        IconText.Text = GetIconText(icon);
        IconBorder.Background = new SolidColorBrush(GetIconBackground(icon));

        _tcsInt = new TaskCompletionSource<int>();
        _tcs = null;
        Visibility = Visibility.Visible;
        PlayEnterAnimation();

        return _tcsInt.Task;
    }

    private Task<bool> Show()
    {
        _tcs = new TaskCompletionSource<bool>();
        _tcsInt = null;
        Visibility = Visibility.Visible;
        PlayEnterAnimation();

        return _tcs.Task;
    }

    private void PlayEnterAnimation()
    {
        var scaleAnim = new DoubleAnimation(0.85, 1.0, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        CardScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, scaleAnim);
        CardScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, scaleAnim);

        var opacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
        BeginAnimation(OpacityProperty, opacityAnim);
    }

    private void Hide(bool result)
    {
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
        fadeOut.Completed += (s, e) =>
        {
            Visibility = Visibility.Collapsed;
            _tcs?.TrySetResult(result);
        };
        BeginAnimation(OpacityProperty, fadeOut);
    }

    private void HideInt(int result)
    {
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
        fadeOut.Completed += (s, e) =>
        {
            Visibility = Visibility.Collapsed;
            _tcsInt?.TrySetResult(result);
        };
        BeginAnimation(OpacityProperty, fadeOut);
    }

    private void PrimaryButton_Click(object sender, RoutedEventArgs e)
    {
        if (_tcsInt != null) HideInt(0);
        else Hide(true);
    }

    private void MiddleButton_Click(object sender, RoutedEventArgs e) => HideInt(1);

    private void SecondaryButton_Click(object sender, RoutedEventArgs e)
    {
        if (_tcsInt != null) HideInt(2);
        else Hide(false);
    }

    private static string GetIconText(OverlayDialogIcon icon) => icon switch
    {
        OverlayDialogIcon.Success => "✅",
        OverlayDialogIcon.Error => "❌",
        OverlayDialogIcon.Warning => "⚠️",
        OverlayDialogIcon.Info => "ℹ️",
        _ => "ℹ️"
    };

    private static Color GetIconBackground(OverlayDialogIcon icon) => icon switch
    {
        OverlayDialogIcon.Success => (Color)ColorConverter.ConvertFromString("#37BDDB"),  // 亮青底
        OverlayDialogIcon.Error   => (Color)ColorConverter.ConvertFromString("#3B1420"),  // 紅底
        OverlayDialogIcon.Warning => (Color)ColorConverter.ConvertFromString("#3B2E14"),  // 琥珀底
        OverlayDialogIcon.Info    => (Color)ColorConverter.ConvertFromString("#2e77ddff"),  // 藍底
        _                         => (Color)ColorConverter.ConvertFromString("#1A2F4D")
    };
}

/// <summary>對話框圖示類型</summary>
public enum OverlayDialogIcon
{
    Info,
    Success,
    Error,
    Warning
}
