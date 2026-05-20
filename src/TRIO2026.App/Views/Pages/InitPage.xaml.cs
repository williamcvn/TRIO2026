using System.Windows.Controls;
using System.Windows.Threading;
using TRIO2026.App.Services;

namespace TRIO2026.App.Views.Pages;

/// <summary>
/// InitPage — 系統初始化倒數頁面
/// 倒數結束後透過事件通知 Shell 切換到 MenuPage
/// </summary>
public partial class InitPage : UserControl
{
    private readonly DispatcherTimer _timer;
    private int _remainingSeconds;

    /// <summary>倒數完成事件</summary>
    public event EventHandler? CountdownCompleted;

    public InitPage(SystemSettingService systemSettings)
    {
        InitializeComponent();

        _remainingSeconds = systemSettings.InitWaitSeconds;
        CountdownText.Text = _remainingSeconds.ToString();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTimerTick;

        Loaded += (s, e) => _timer.Start();
        Unloaded += (s, e) => _timer.Stop();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _remainingSeconds--;
        CountdownText.Text = _remainingSeconds.ToString();

        if (_remainingSeconds <= 0)
        {
            _timer.Stop();
            CountdownCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
