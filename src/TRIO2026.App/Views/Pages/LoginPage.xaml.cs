using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TRIO2026.App.Services;
using TRIO2026.App.ViewModels;

namespace TRIO2026.App.Views.Pages;

/// <summary>
/// LoginPage — 登入頁面（UserControl）
/// 由 AppShell 託管，登入成功後透過事件通知 Shell 切換頁面
/// </summary>
public partial class LoginPage : UserControl
{
    private readonly LoginViewModel _viewModel;
    private readonly SystemSettingService _settings;

    /// <summary>登入成功事件 — Shell 收到後切換到 MenuPage</summary>
    public event EventHandler? LoginSucceeded;

    /// <summary>關閉請求事件 — Shell 收到後關閉應用程式</summary>
    public event EventHandler? CloseRequested;

    public LoginPage(LoginViewModel viewModel, SystemSettingService settings)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _settings = settings;
        DataContext = _viewModel;

        // 訂閱登入成功事件
        _viewModel.LoginSucceeded += OnLoginSucceeded;

        // 訂閱抖動動畫
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(LoginViewModel.ShowShakeAnimation) && _viewModel.ShowShakeAnimation)
            {
                var storyboard = (Storyboard)FindResource("ShakeAnimation");
                storyboard.Begin(LoginCard);
            }
        };

        // 關閉按鈕（受 DB 控制 — 讀取 system_config.db）
        CloseButton.Visibility = _settings.CloseButtonEnabled
            ? Visibility.Visible
            : Visibility.Collapsed;

        // 載入記住的密碼
        Loaded += (s, e) =>
        {
            if (!string.IsNullOrEmpty(_viewModel.Password))
            {
                PasswordBox.Password = _viewModel.Password;
            }
            UsernameBox.Focus();
        };
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.Password = PasswordBox.Password;
        }
    }

    private void InputBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Parent is Border border)
        {
            border.BorderBrush = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#42A5F5"));
            border.BorderThickness = new Thickness(2);
        }
    }

    private void InputBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Parent is Border border)
        {
            border.BorderBrush = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#4A6A9A"));
            border.BorderThickness = new Thickness(1.5);
        }
    }

    private void OnLoginSucceeded(object? sender, EventArgs e)
    {
        LoginSucceeded?.Invoke(this, EventArgs.Empty);
    }
}
