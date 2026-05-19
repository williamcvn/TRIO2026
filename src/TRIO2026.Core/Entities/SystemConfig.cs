namespace TRIO2026.Core.Entities;

/// <summary>
/// 通用配置表，取代舊系統 12 個 .ini 檔案的 Key-Value 配置。
/// 對應資料庫: trio240plus_config.db
/// </summary>
public class SystemConfig
{
    /// <summary>主鍵</summary>
    public int Id { get; set; }

    /// <summary>配置分類（對應原 .ini 檔名：motor, area_position, temperature, optics, pipette, trio_info, tube, camera, system, maintenance）</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>參數名稱</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>參數值</summary>
    public string? Value { get; set; }

    /// <summary>值的型別提示：int / float / string / bool</summary>
    public string DataType { get; set; } = "string";

    /// <summary>參數說明</summary>
    public string? Description { get; set; }

    /// <summary>修改時間（ISO8601）</summary>
    public string ModifiedAt { get; set; } = string.Empty;

    /// <summary>修改者</summary>
    public string? ModifiedBy { get; set; }
}
