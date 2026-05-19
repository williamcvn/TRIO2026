namespace TRIO2026.Core.Enums;

/// <summary>
/// 角色等級列舉
/// </summary>
public enum RoleLevel
{
    /// <summary>操作員 — 基本操作權限</summary>
    Operator = 1,
    /// <summary>Service 工程師 — 系統設定 + 進階維護</summary>
    Service = 2,
    /// <summary>管理員 — 全部權限（含帳號管理）</summary>
    Admin = 3
}
