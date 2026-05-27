namespace TRIO2026.Core.Entities;

/// <summary>
/// 報表快照表，記錄每次報告匯出的完整資訊。
/// 對應資料庫: data.db
/// 
/// 支援 Excel 與 PDF 雙匯出格式。
/// 
/// 製作者: Office of William
/// </summary>
public class ReportSnapshot
{
    /// <summary>主鍵</summary>
    public int Id { get; set; }

    /// <summary>所屬檢測記錄 ID（FK → TestRecord.Id）</summary>
    public int TestRecordId { get; set; }

    /// <summary>報表類型：IntelliPlex / Custom</summary>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>產生時間（ISO 8601）</summary>
    public string GeneratedAt { get; set; } = string.Empty;

    // ── 產生者審計 ──

    /// <summary>產生者 User.Id</summary>
    public int? GeneratedByUserId { get; set; }

    /// <summary>產生者帳號（快照）</summary>
    public string? GeneratedByUsername { get; set; }

    // ── 報表內容 ──

    /// <summary>報表內容（JSON 格式，可用於重建報表）</summary>
    public string? ContentJson { get; set; }

    /// <summary>PDF 二進位（選用）</summary>
    public byte[]? PdfBlob { get; set; }

    /// <summary>Excel 檔案路徑（相對於 Database/reports/）</summary>
    public string? ExcelFilePath { get; set; }

    /// <summary>PDF 檔案路徑（相對於 Database/reports/）</summary>
    public string? PdfFilePath { get; set; }

    // ── 導航屬性 ──

    /// <summary>導航屬性：所屬檢測記錄</summary>
    public TestRecord TestRecord { get; set; } = null!;
}
