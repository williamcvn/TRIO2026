using TRIO2026.Core.Entities;
using TRIO2026.Core.Enums;

namespace TRIO2026.App.Services;

/// <summary>
/// 會話管理服務 — 管理當前登入使用者狀態
/// 
/// 使用 User 實體（main.db）
/// 
/// 製作者: Office of William
/// </summary>
public class SessionService
{
    /// <summary>當前登入的使用者（null 表示未登入）</summary>
    public User? CurrentUser { get; private set; }

    /// <summary>是否已認證</summary>
    public bool IsAuthenticated => CurrentUser != null;

    /// <summary>是否為免登入模式（Guest Session）</summary>
    public bool IsGuestMode { get; private set; }

    /// <summary>當前使用者的角色等級</summary>
    public RoleLevel CurrentRole => IsAuthenticated
        ? (RoleLevel)CurrentUser!.RoleLevel
        : 0;

    /// <summary>會話變更事件</summary>
    public event EventHandler? SessionChanged;

    /// <summary>設定當前使用者（登入成功後呼叫）</summary>
    public void SetCurrentUser(User user)
    {
        CurrentUser = user;
        IsGuestMode = false;
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>清除會話（登出）</summary>
    public void ClearSession()
    {
        CurrentUser = null;
        IsGuestMode = false;
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>免登入模式 — 載入 DB Guest 帳號並套用系統設定</summary>
    public void SetGuestSession(User guestUser, string displayName)
    {
        guestUser.RoleLevel = (int)RoleLevel.Operator; // 固定 Operator
        guestUser.DisplayName = displayName;
        CurrentUser = guestUser;
        IsGuestMode = true;
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>檢查當前使用者是否有指定權限等級</summary>
    public bool HasPermission(RoleLevel required)
    {
        return IsAuthenticated && CurrentRole >= required;
    }
}
