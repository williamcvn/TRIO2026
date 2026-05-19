namespace TRIO2026.Core.Entities;

/// <summary>
/// PNID 編碼映射表，取代 flowinfo.ini [PNID] Section。
/// 對應資料庫: trio240plus_main.db
/// </summary>
public class PnidMapping
{
    /// <summary>主鍵</summary>
    public int Id { get; set; }

    /// <summary>PNID 編碼（如 "82004", "83015"）</summary>
    public string PnidCode { get; set; } = string.Empty;

    /// <summary>英文說明</summary>
    public string? DescriptionEn { get; set; }

    /// <summary>中文說明</summary>
    public string? DescriptionZh { get; set; }

    /// <summary>關聯的 ProductCode</summary>
    public string? LinkedProductCode { get; set; }
}
