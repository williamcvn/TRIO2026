namespace TRIO2026.Core.Entities;

/// <summary>
/// UV 照射時間選項 — 對應資料庫: trio240plus_config.db → UvTimerOption 表
/// 
/// 設計目標：
///   1. 管理員可自由新增/移除照射時間選項
///   2. 透過 IsEnabled 控制哪些選項在 UI 上顯示
///   3. 透過 IsDefault 指定預設選中項目（僅一筆為 true）
///   4. 透過 SortOrder 控制左右方向鍵的選取順序
///   5. DisplayLabel 支援多語系自訂顯示文字
/// 
/// UI 操作流程：
///   使用者在觸控面板上透過左右方向鍵，依 SortOrder 順序
///   在 IsEnabled=1 的選項間切換，預設停在 IsDefault=1 的項目。
/// </summary>
public class UvTimerOption
{
    /// <summary>主鍵（自動遞增）</summary>
    public int Id { get; set; }

    /// <summary>
    /// 照射持續時間（秒）
    /// 範例: 900=15分鐘, 1800=30分鐘, 2700=45分鐘, 3600=60分鐘
    /// </summary>
    public int DurationSeconds { get; set; }

    /// <summary>
    /// UI 顯示標籤（mm:ss 格式，支援多語系自訂）
    /// 範例: "15:00", "30:00", "45:00", "60:00"
    /// </summary>
    public string DisplayLabel { get; set; } = string.Empty;

    /// <summary>
    /// 是否在 UI 上啟用顯示
    /// 0 = 隱藏（保留在 DB 但不出現在選項中）
    /// 1 = 顯示（納入左右方向鍵可選範圍）
    /// </summary>
    public int IsEnabled { get; set; } = 1;

    /// <summary>
    /// 是否為預設選項（進入 UV 頁面時預設選中的項目）
    /// 整張表只應有一筆 IsDefault=1，若多筆則取 SortOrder 最小者
    /// </summary>
    public int IsDefault { get; set; } = 0;

    /// <summary>
    /// 排序順序（左右方向鍵依此順序切換）
    /// 數值越小越靠左，數值越大越靠右
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 備註說明（管理用途，UI 不顯示）
    /// </summary>
    public string? Description { get; set; }
}
