namespace TRIO2026.Core.Entities;

/// <summary>
/// 使用者帳號表，取代舊系統 userinfo.db。
/// 密碼使用 BCrypt 雜湊存儲，禁止明碼。
/// 對應資料庫: trio240plus_main.db
/// </summary>
public class UserAccount
{
    /// <summary>主鍵</summary>
    public int Id { get; set; }

    /// <summary>使用者帳號</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>BCrypt 密碼雜湊（禁止明碼）</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>角色等級：1=操作員, 2=Service工程師, 3=管理員</summary>
    public int RoleLevel { get; set; } = 1;

    /// <summary>帳號狀態：0=停用, 1=啟用</summary>
    public int IsActive { get; set; } = 1;

    /// <summary>建立時間（ISO8601）</summary>
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>最後登入時間</summary>
    public string? LastLoginAt { get; set; }

    // === 新增安全欄位 ===

    /// <summary>連續登入失敗次數（預設 0，成功登入時重設）</summary>
    public int FailedLoginCount { get; set; } = 0;

    /// <summary>帳號鎖定到期時間（ISO8601），null 表示未鎖定</summary>
    public string? LockedUntil { get; set; }

    /// <summary>密碼最後變更時間（ISO8601）</summary>
    public string? PasswordChangedAt { get; set; }

    /// <summary>顯示名稱</summary>
    public string? DisplayName { get; set; }

    /// <summary>使用者頭像（PNG 二進位）</summary>
    public byte[]? AvatarImage { get; set; }

    // === 導航屬性 ===

    /// <summary>角色定義（透過 RoleLevel 關聯）</summary>
    public RoleDefinition? Role { get; set; }
}
