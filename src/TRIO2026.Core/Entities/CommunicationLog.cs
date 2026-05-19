namespace TRIO2026.Core.Entities;

/// <summary>
/// Modbus 通訊記錄表。
/// 對應資料庫: trio240plus_log.db
/// </summary>
public class CommunicationLog
{
    /// <summary>主鍵</summary>
    public int Id { get; set; }

    /// <summary>時間戳（ISO8601）</summary>
    public string Timestamp { get; set; } = string.Empty;

    /// <summary>方向：Send / Receive</summary>
    public string Direction { get; set; } = string.Empty;

    /// <summary>Modbus 功能碼（0x03, 0x06, 0x10）</summary>
    public int? FunctionCode { get; set; }

    /// <summary>暫存器位址</summary>
    public int? Address { get; set; }

    /// <summary>原始資料（十六進位）</summary>
    public string? DataHex { get; set; }

    /// <summary>是否為錯誤回覆</summary>
    public int IsError { get; set; }
}
