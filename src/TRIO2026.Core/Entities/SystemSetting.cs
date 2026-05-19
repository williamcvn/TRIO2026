namespace TRIO2026.Core.Entities;

/// <summary>
/// 系統設定（system_config.db — SystemSetting 表）
/// 
/// 通用 Key-Value 設定表，用於存放各種系統級參數。
/// Category 用來分組，Key 為設定名稱，Value 為設定值。
/// 
/// 設定分類：
///   - UserMenu   使用者選單相關設定
///   - System     系統全域設定
///   - UI         UI 顯示設定（預留）
/// 
/// 製作者: Office of William
/// </summary>
public class SystemSetting
{
    /// <summary>主鍵</summary>
    public int Id { get; set; }

    /// <summary>設定分類（如 UserMenu, System）</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>設定鍵值</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>設定值</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>說明描述</summary>
    public string? Description { get; set; }

    /// <summary>備註（實作狀態追蹤等）</summary>
    public string? Remark { get; set; }
}
