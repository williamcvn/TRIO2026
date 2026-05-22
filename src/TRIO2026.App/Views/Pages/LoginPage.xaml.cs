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

        // 使用者下拉選單（受 DB 控制 — show_user_dropdown）
        _viewModel.ShowUserDropdown = _settings.ShowUserDropdown;

        // 多語系
        ApplyLocalization();

        // 載入記住的密碼 + 使用者清單
        Loaded += async (s, e) =>
        {
            if (!string.IsNullOrEmpty(_viewModel.Password))
            {
                PasswordBox.Password = _viewModel.Password;
            }

            if (_viewModel.ShowUserDropdown)
            {
                await _viewModel.LoadUsersAsync();
                // 如果有記住的帳號，自動選中對應的使用者
                if (!string.IsNullOrEmpty(_viewModel.Username))
                {
                    var match = _viewModel.UserList
                        .FirstOrDefault(u => u.Username == _viewModel.Username);
                    if (match != null)
                        _viewModel.SelectedUser = match;
                }
                PasswordBox.Focus();
            }
            else
            {
                UsernameBox.Focus();
            }
        };
    }

    /// <summary>刷新多語系與 UI 顯示</summary>
    public void RefreshDisplay()
    {
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        var loc = LocalizationService.Instance;
        LblUsername.Text = loc["Login.Username"];
        LblPassword.Text = loc["Login.Password"];
        ChkRemember.Content = loc["Login.RememberPassword"];
        BtnLogin.Content = loc["Login.Submit"];
        CloseButton.ToolTip = loc["Login.Close"];
    }

    /// <summary>下拉選單選擇變更 — 自動聚焦到密碼框</summary>
    private void UserDropdown_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (UserDropdown.SelectedItem != null)
        {
            PasswordBox.Password = ""; // 清除舊密碼
            PasswordBox.Focus();
        }
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
