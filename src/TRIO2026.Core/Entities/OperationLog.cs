namespace TRIO2026.Core.Entities;

/// <summary>
/// 使用者操作日誌表。
/// 對應資料庫: trio240plus_log.db
/// </summary>
public class OperationLog
{
    /// <summary>主鍵</summary>
    public int Id { get; set; }

    /// <summary>時間戳（ISO8601）</summary>
    public string Timestamp { get; set; } = string.Empty;

    /// <summary>日誌等級：Info / Warning / Error</summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>分類：UI / Flow / Modbus / System</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>操作者</summary>
    public string? UserName { get; set; }

    /// <summary>動作描述</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>詳細資訊</summary>
    public string? Detail { get; set; }
}
