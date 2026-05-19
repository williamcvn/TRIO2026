namespace TRIO2026.Core.Entities;

/// <summary>
/// 檢測運行記錄表，每次運行一筆。
/// 對應資料庫: trio240plus_data.db
/// </summary>
public class TestRecord
{
    /// <summary>主鍵</summary>
    public int Id { get; set; }

    /// <summary>運行批次 ID（時間戳生成）</summary>
    public string RunId { get; set; } = string.Empty;

    /// <summary>執行的流程名稱</summary>
    public string FlowName { get; set; } = string.Empty;

    /// <summary>產品編碼</summary>
    public string? ProductCode { get; set; }

    /// <summary>操作員</summary>
    public string? OperatorName { get; set; }

    /// <summary>樣本數量</summary>
    public int? SampleCount { get; set; }

    /// <summary>開始時間</summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>結束時間</summary>
    public string? EndTime { get; set; }

    /// <summary>狀態：Running / Completed / Error / Aborted</summary>
    public string Status { get; set; } = "Running";

    /// <summary>錯誤碼</summary>
    public string? ErrorCode { get; set; }

    /// <summary>錯誤訊息</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>導航屬性：樣本結果集合</summary>
    public ICollection<SampleResult> SampleResults { get; set; } = new List<SampleResult>();

    /// <summary>導航屬性：報表快照集合</summary>
    public ICollection<ReportSnapshot> ReportSnapshots { get; set; } = new List<ReportSnapshot>();
}
