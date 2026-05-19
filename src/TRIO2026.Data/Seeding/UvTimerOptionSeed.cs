using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Seeding;

/// <summary>
/// UV 照射時間選項種子資料
/// 
/// 管理說明：
///   - 每一筆代表一個 UV 照射時間選項
///   - IsEnabled=1 的選項會出現在 UI 的左右方向鍵選擇中
///   - IsDefault=1 為進入 UV 頁面時的預設選中項（僅一筆）
///   - SortOrder 決定左右切換順序（小→左, 大→右）
///   - 新增選項：插入新 row，設定 DurationSeconds/DisplayLabel/SortOrder
///   - 停用選項：將 IsEnabled 改為 0（不刪除，保留歷史）
///   - 變更預設：將舊預設的 IsDefault 改 0，新預設改 1
/// 
/// 製作者: Office of William
/// </summary>
public static class UvTimerOptionSeed
{
    public static List<UvTimerOption> GetSeedData()
    {
        return new List<UvTimerOption>
        {
            new()
            {
                Id = 1,
                DurationSeconds = 900,       // 15 分鐘
                DisplayLabel = "15:00",
                IsEnabled = 1,
                IsDefault = 1,               // ← 預設選項
                SortOrder = 10,
                Description = "15 minutes UV decontamination"
            },
            new()
            {
                Id = 2,
                DurationSeconds = 1800,      // 30 分鐘
                DisplayLabel = "30:00",
                IsEnabled = 1,
                IsDefault = 0,
                SortOrder = 20,
                Description = "30 minutes UV decontamination"
            },
            new()
            {
                Id = 3,
                DurationSeconds = 2700,      // 45 分鐘
                DisplayLabel = "45:00",
                IsEnabled = 1,
                IsDefault = 0,
                SortOrder = 30,
                Description = "45 minutes UV decontamination"
            },
            new()
            {
                Id = 4,
                DurationSeconds = 3600,      // 60 分鐘
                DisplayLabel = "60:00",
                IsEnabled = 1,
                IsDefault = 0,
                SortOrder = 40,
                Description = "60 minutes UV decontamination"
            },
        };
    }
}
