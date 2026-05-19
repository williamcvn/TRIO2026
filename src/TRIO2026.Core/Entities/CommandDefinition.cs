namespace TRIO2026.Core.Entities;

/// <summary>
/// 指令定義表，取代 funstep.ini 的 58 個指令。
/// 每個指令可帶 5 組參數（Arg0~Arg4），每組有 Type/Label/Options。
/// 對應資料庫: trio240plus_config.db
/// </summary>
public class CommandDefinition
{
    /// <summary>指令 ID（0~57，對應 X_Flow_t 列舉）</summary>
    public int Id { get; set; }

    /// <summary>指令名稱（如「移動槍頭」）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>參數0類型：0=無, 1=下拉, 2=數值, 3=字串</summary>
    public int Arg0Type { get; set; }

    /// <summary>參數0標籤</summary>
    public string? Arg0Label { get; set; }

    /// <summary>參數0選項（JSON 陣列）</summary>
    public string? Arg0Options { get; set; }

    public int Arg1Type { get; set; }
    public string? Arg1Label { get; set; }
    public string? Arg1Options { get; set; }

    public int Arg2Type { get; set; }
    public string? Arg2Label { get; set; }
    public string? Arg2Options { get; set; }

    public int Arg3Type { get; set; }
    public string? Arg3Label { get; set; }
    public string? Arg3Options { get; set; }

    public int Arg4Type { get; set; }
    public string? Arg4Label { get; set; }
    public string? Arg4Options { get; set; }

    /// <summary>指令說明</summary>
    public string? Note { get; set; }

    /// <summary>顯示格式模板（如「移動槍頭到%1」）</summary>
    public string? DisplayFormat { get; set; }
}
