namespace TRIO2026.Core.Entities;

/// <summary>
/// 角色定義表 — 管理角色等級與說明
/// 與 UserAccount.RoleLevel 建立外鍵關聯
/// </summary>
public class RoleDefinition
{
    /// <summary>角色等級（主鍵）：1=操作員, 2=Service工程師, 3=管理員</summary>
    public int Level { get; set; }

    /// <summary>角色代碼（唯一）：Operator, Service, Admin</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>角色顯示名稱：操作員, Service 工程師, 管理員</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>角色說明</summary>
    public string? Description { get; set; }

    /// <summary>建立時間（ISO8601）</summary>
    public string CreatedAt { get; set; } = string.Empty;

    // === 導航屬性 ===

    /// <summary>擁有此角色的使用者</summary>
    public ICollection<UserAccount> Users { get; set; } = new List<UserAccount>();
}
