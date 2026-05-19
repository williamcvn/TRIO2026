namespace TRIO2026.Core.Entities;

/// <summary>
/// 樣本結果表，每個樣本一筆。
/// 對應資料庫: trio240plus_data.db
/// </summary>
public class SampleResult
{
    /// <summary>主鍵</summary>
    public int Id { get; set; }

    /// <summary>所屬檢測記錄 ID（FK → TestRecord.Id）</summary>
    public int TestRecordId { get; set; }

    /// <summary>樣本條碼</summary>
    public string? SampleBarcode { get; set; }

    /// <summary>樣本位置編號</summary>
    public int? SamplePosition { get; set; }

    /// <summary>濃度結果</summary>
    public double? Concentration { get; set; }

    /// <summary>體積（μL）</summary>
    public double? Volume { get; set; }

    /// <summary>品質標記：Pass / Fail / Recheck</summary>
    public string? QualityFlag { get; set; }

    /// <summary>光學原始數據（JSON）</summary>
    public string? RawDataJson { get; set; }

    /// <summary>建立時間</summary>
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>導航屬性：所屬檢測記錄</summary>
    public TestRecord TestRecord { get; set; } = null!;
}
