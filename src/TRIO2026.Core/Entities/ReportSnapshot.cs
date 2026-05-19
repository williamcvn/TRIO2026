namespace TRIO2026.Core.Entities;

/// <summary>
/// 報表快照表，用於匯出報表。
/// 對應資料庫: trio240plus_data.db
/// </summary>
public class ReportSnapshot
{
    /// <summary>主鍵</summary>
    public int Id { get; set; }

    /// <summary>所屬檢測記錄 ID（FK → TestRecord.Id）</summary>
    public int TestRecordId { get; set; }

    /// <summary>報表類型</summary>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>產生時間</summary>
    public string GeneratedAt { get; set; } = string.Empty;

    /// <summary>報表內容（JSON 格式）</summary>
    public string? ContentJson { get; set; }

    /// <summary>PDF 二進位（選用）</summary>
    public byte[]? PdfBlob { get; set; }

    /// <summary>導航屬性：所屬檢測記錄</summary>
    public TestRecord TestRecord { get; set; } = null!;
}
