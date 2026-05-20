using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using TRIO2026.App.Helpers;
using TRIO2026.App.Services;
using TRIO2026.Core.Enums;

namespace TRIO2026.App.ViewModels;

/// <summary>
/// 登入頁面 ViewModel — MVVM 模式
/// </summary>
public class LoginViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly SessionService _sessionService;
    private readonly TokenService _tokenService;

    public LoginViewModel(AuthService authService, SessionService sessionService, TokenService tokenService)
    {
        _authService = authService;
        _sessionService = sessionService;
        _tokenService = tokenService;

        LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync, _ => CanLogin);

        // 螢幕偵測
        IsTouchScreen = ScreenDetector.IsTouchSupported;
        ScreenInfo = $"{ScreenDetector.ScreenWidth}×{ScreenDetector.ScreenHeight}" +
                     (IsTouchScreen ? " (觸控)" : " (非觸控)");

        // 嘗試載入記住的密碼
        LoadRememberedCredentials();
    }

    // ===== 繫結屬性 =====

    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set { SetProperty(ref _username, value); OnPropertyChanged(nameof(CanLogin)); }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set { SetProperty(ref _password, value); OnPropertyChanged(nameof(CanLogin)); }
    }

    private bool _rememberMe;
    public bool RememberMe
    {
        get => _rememberMe;
        set => SetProperty(ref _rememberMe, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { SetProperty(ref _errorMessage, value); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { SetProperty(ref _isLoading, value); OnPropertyChanged(nameof(CanLogin)); }
    }

    private bool _showShakeAnimation;
    public bool ShowShakeAnimation
    {
        get => _showShakeAnimation;
        set => SetProperty(ref _showShakeAnimation, value);
    }

    private ImageSource? _avatarImage;
    public ImageSource? AvatarImage
    {
        get => _avatarImage;
        set => SetProperty(ref _avatarImage, value);
    }

    public bool IsTouchScreen { get; }
    public string ScreenInfo { get; }

    public bool CanLogin => !string.IsNullOrWhiteSpace(Username) &&
                            !string.IsNullOrWhiteSpace(Password) &&
                            !IsLoading;

    // ===== Commands =====

    public ICommand LoginCommand { get; }

    /// <summary>登入成功事件（由 Window 訂閱以關閉或導航）</summary>
    public event EventHandler? LoginSucceeded;

    /// <summary>取得當前使用者顯示資訊（名稱 + 角色）</summary>
    public (string name, string role) CurrentUserDisplay
    {
        get
        {
            var user = _sessionService.CurrentUser;
            if (user == null) return ("", "");
            var displayName = user.DisplayName ?? user.Username;
            var roleName = user.RoleLevel switch { 1 => "Operator", 2 => "Service", 3 => "Admin", _ => $"Level {user.RoleLevel}" };
            return (displayName, $"{roleName} (Level {user.RoleLevel})");
        }
    }

    // ===== 邏輯 =====

    private async Task ExecuteLoginAsync(object? parameter)
    {
        ErrorMessage = string.Empty;
        IsLoading = true;

        try
        {
            var (result, user) = await _authService.LoginAsync(Username, Password);

            switch (result)
            {
                case AuthResult.Success:
                    _sessionService.SetCurrentUser(user!);

                    if (RememberMe)
                        _tokenService.SaveRememberedCredentials(Username, Password);
                    else
                        _tokenService.ClearRememberedCredentials();

                    // 操作追蹤：登入成功
                    EventLogService.Instance.LogAuth("Login", Username, true,
                        $"RoleLevel={user!.RoleLevel}");

                    // 切換至使用者的偏好語系
                    if (!string.IsNullOrEmpty(user.LanguagePreference))
                    {
                        await LocalizationService.Instance.SwitchLanguageAsync(user.LanguagePreference);
                    }

                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
                    break;

                case AuthResult.UserNotFound:
                case AuthResult.WrongPassword:
                    ErrorMessage = "帳號或密碼錯誤";
                    EventLogService.Instance.LogAuth("Login", Username, false, $"Reason={result}");
                    TriggerShake();
                    break;

                case AuthResult.AccountDisabled:
                    ErrorMessage = "此帳號已停用，請聯絡管理員";
                    EventLogService.Instance.LogAuth("Login", Username, false, "Reason=AccountDisabled");
                    TriggerShake();
                    break;

                case AuthResult.AccountLocked:
                    ErrorMessage = "帳號已鎖定，請稍後再試";
                    EventLogService.Instance.LogAuth("Login", Username, false, "Reason=AccountLocked");
                    TriggerShake();
                    break;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"系統錯誤：{ex.Message}";
            EventLogService.Instance?.LogException("Auth", "LoginViewModel", ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadRememberedCredentials()
    {
        var saved = _tokenService.LoadRememberedCredentials();
        if (saved.HasValue)
        {
            Username = saved.Value.Username;
            Password = saved.Value.Password;
            RememberMe = true;
        }
    }

    private async void TriggerShake()
    {
        ShowShakeAnimation = true;
        await Task.Delay(500);
        ShowShakeAnimation = false;
    }
}
