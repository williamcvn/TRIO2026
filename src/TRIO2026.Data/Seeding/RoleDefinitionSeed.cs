using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Seeding;

/// <summary>
/// 角色定義種子資料
/// </summary>
public static class RoleDefinitionSeed
{
    public static List<RoleDefinition> GetSeedData()
    {
        var now = DateTime.UtcNow.ToString("o");

        return new List<RoleDefinition>
        {
            new()
            {
                Level = 1,
                Code = "Operator",
                DisplayName = "操作員",
                Description = "基本操作權限 — 執行流程、查看報表",
                CreatedAt = now,
            },
            new()
            {
                Level = 2,
                Code = "Service",
                DisplayName = "Service 工程師",
                Description = "系統設定 + 進階維護 — 校正、參數調整、流程編輯",
                CreatedAt = now,
            },
            new()
            {
                Level = 3,
                Code = "Admin",
                DisplayName = "管理員",
                Description = "全部權限 — 帳號管理、系統組態、資料匯出",
                CreatedAt = now,
            },
        };
    }
}
