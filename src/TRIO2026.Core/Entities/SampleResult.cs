namespace TRIO2026.Core.Entities;

/// <summary>
/// 樣本結果表，每個樣本一筆。
/// 對應資料庫: data.db
/// 
/// 記錄每個樣本位置的濃度量測結果、PCR 孔位分配、
/// 使用者輸入的 Sample ID 與 Elution Tube ID。
/// 
/// 製作者: Office of William
/// </summary>
public class SampleResult
{
    /// <summary>主鍵</summary>
    public int Id { get; set; }

    /// <summary>所屬檢測記錄 ID（FK → TestRecord.Id）</summary>
    public int TestRecordId { get; set; }

    // ── 樣本識別 ──

    /// <summary>樣本位置編號（1-24，對應機台孔位）</summary>
    public int? SamplePosition { get; set; }

    /// <summary>樣本條碼（掃碼器讀取）</summary>
    public string? SampleBarcode { get; set; }

    /// <summary>使用者輸入的 Sample ID（對應 Excel F 欄）</summary>
    public string? SampleId { get; set; }

    /// <summary>洗脫管 ID（對應 Excel G 欄）</summary>
    public string? ElutionTubeId { get; set; }

    // ── 量測結果 ──

    /// <summary>濃度結果（ng/μL）</summary>
    public double? Concentration { get; set; }

    /// <summary>濃度顯示文字（如 "< 1.00"，保留原始顯示格式）</summary>
    public string? ConcentrationDisplay { get; set; }

    /// <summary>使用的洗脫量（μL）</summary>
    public double? UtilizedElutedVolume { get; set; }

    /// <summary>體積（μL，通用欄位）</summary>
    public double? Volume { get; set; }

    // ── PCR 孔位分配 ──

    /// <summary>PCR 孔位 — Kit1 / Rxn1（如 A1, B1...）</summary>
    public string? PcrWellKit1 { get; set; }

    /// <summary>PCR 孔位 — Kit2 / Rxn2</summary>
    public string? PcrWellKit2 { get; set; }

    /// <summary>PCR 孔位 — Rxn3（Custom Report 專用）</summary>
    public string? PcrWellRxn3 { get; set; }

    /// <summary>PCR 孔位 — Rxn4（Custom Report 專用）</summary>
    public string? PcrWellRxn4 { get; set; }

    // ── 品質與原始數據 ──

    /// <summary>品質標記：Pass / Fail / Recheck</summary>
    public string? QualityFlag { get; set; }

    /// <summary>光學原始數據（JSON）</summary>
    public string? RawDataJson { get; set; }

    /// <summary>建立時間（ISO 8601）</summary>
    public string CreatedAt { get; set; } = string.Empty;

    // ── 導航屬性 ──

    /// <summary>導航屬性：所屬檢測記錄</summary>
    public TestRecord TestRecord { get; set; } = null!;
}
