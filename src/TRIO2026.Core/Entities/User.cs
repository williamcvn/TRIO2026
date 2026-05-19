namespace TRIO2026.Core.Entities;

/// <summary>
/// 正式使用者帳號表 — 對應 main.db
/// 
/// 從 trio240plus_main.db 的 UserAccount 演進而來，
/// 新增了醫療級系統所需的完整欄位。
/// 
/// 欄位差異（vs 舊版 UserAccount）：
///   - 新增 Email（聯絡用）
///   - 新增 Department（部門/科別）
///   - 新增 EmployeeId（員工編號）
///   - 新增 CreatedBy / UpdatedAt / UpdatedBy（稽核追蹤）
///   - 新增 Notes（管理備註）
///   - 新增 ForcePasswordChange（強制密碼變更旗標）
///   - 新增 PasswordExpiryDays（密碼效期天數）
///   - 保留 AvatarImage、安全欄位、角色等級
/// 
/// 製作者: Office of William
/// </summary>
public class User
{
    public int Id { get; set; }

    // ── 身分資訊 ──
    /// <summary>登入帳號（唯一）</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>顯示名稱</summary>
    public string? DisplayName { get; set; }

    /// <summary>員工編號</summary>
    public string? EmployeeId { get; set; }

    /// <summary>Email（通知或聯絡用）</summary>
    public string? Email { get; set; }

    /// <summary>部門/科別</summary>
    public string? Department { get; set; }

    // ── 安全性 ──
    /// <summary>BCrypt 密碼雜湊（禁止明碼）</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>角色等級：1=Operator, 2=Service, 3=Admin</summary>
    public int RoleLevel { get; set; } = 1;

    /// <summary>帳號狀態：0=停用, 1=啟用</summary>
    public int IsActive { get; set; } = 1;

    /// <summary>連續登入失敗次數（成功登入時重設為 0）</summary>
    public int FailedLoginCount { get; set; } = 0;

    /// <summary>帳號鎖定到期時間（ISO8601），null 表示未鎖定</summary>
    public string? LockedUntil { get; set; }

    /// <summary>密碼最後變更時間（ISO8601）</summary>
    public string? PasswordChangedAt { get; set; }

    /// <summary>是否強制下次登入時變更密碼</summary>
    public int ForcePasswordChange { get; set; } = 0;

    /// <summary>密碼效期天數（0=永不過期）</summary>
    public int PasswordExpiryDays { get; set; } = 0;

    // ── 時間戳記 ──
    /// <summary>最後登入時間（ISO8601）</summary>
    public string? LastLoginAt { get; set; }

    /// <summary>建立時間（ISO8601）</summary>
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>建立者（帳號名稱或 "SYSTEM"）</summary>
    public string CreatedBy { get; set; } = "SYSTEM";

    /// <summary>最後更新時間（ISO8601）</summary>
    public string? UpdatedAt { get; set; }

    /// <summary>最後更新者</summary>
    public string? UpdatedBy { get; set; }

    // ── 其他 ──
    /// <summary>使用者頭像（PNG 二進位）</summary>
    public byte[]? AvatarImage { get; set; }

    /// <summary>管理備註</summary>
    public string? Notes { get; set; }

    /// <summary>個人語系偏好 (e.g., "zh-TW", "en")</summary>
    public string? LanguagePreference { get; set; }
}
