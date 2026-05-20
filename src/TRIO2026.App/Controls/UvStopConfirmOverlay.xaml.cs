using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TRIO2026.App.Controls;

/// <summary>
/// UV 停止確認 Overlay — 八邊形 info icon 風格
/// 
/// 用途：UV 倒數期間按 STOP 時彈出確認
/// No = 關閉對話框，倒數繼續
/// YES = 確認停止 UV
/// Dismiss() = 倒數結束時自動關閉（由外部呼叫）
/// 
/// 製作者: Office of William
/// </summary>
public partial class UvStopConfirmOverlay : UserControl
{
    private TaskCompletionSource<bool>? _tcs;

    public UvStopConfirmOverlay()
    {
        InitializeComponent();
    }

    /// <summary>是否正在顯示中</summary>
    public bool IsShowing => Visibility == Visibility.Visible;

    /// <summary>顯示確認對話框，回傳 true=確認停止, false=取消</summary>
    public Task<bool> ShowAsync()
    {
        _tcs = new TaskCompletionSource<bool>();
        Visibility = Visibility.Visible;
        PlayEnterAnimation();
        return _tcs.Task;
    }

    /// <summary>外部強制關閉（倒數結束時自動呼叫，回傳 false 表示不停止）</summary>
    public void Dismiss()
    {
        if (!IsShowing) return;
        Visibility = Visibility.Collapsed;
        _tcs?.TrySetResult(false);
    }

    private void NoButton_Click(object sender, RoutedEventArgs e) => Hide(false);
    private void YesButton_Click(object sender, RoutedEventArgs e) => Hide(true);

    private void Hide(bool result)
    {
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
        fadeOut.Completed += (s, e) =>
        {
            Visibility = Visibility.Collapsed;
            _tcs?.TrySetResult(result);
        };
        BeginAnimation(OpacityProperty, fadeOut);
    }

    private void PlayEnterAnimation()
    {
        Opacity = 0;
        CardScale.ScaleX = 0.9;
        CardScale.ScaleY = 0.9;

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
        var scaleX = new DoubleAnimation(0.9, 1, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        var scaleY = new DoubleAnimation(0.9, 1, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        BeginAnimation(OpacityProperty, fadeIn);
        CardScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
        CardScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
    }
}
