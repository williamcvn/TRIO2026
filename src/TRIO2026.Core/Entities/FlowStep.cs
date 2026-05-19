namespace TRIO2026.Core.Entities;

/// <summary>
/// 流程步驟表，取代 .flow 檔案內容。
/// 每個步驟對應一個指令（CommandId 0~57），可帶 5 個參數。
/// 對應資料庫: trio240plus_main.db
/// </summary>
public class FlowStep
{
    /// <summary>主鍵</summary>
    public int Id { get; set; }

    /// <summary>所屬流程定義 ID（FK → FlowDefinition.Id）</summary>
    public int FlowDefinitionId { get; set; }

    /// <summary>步驟序號（從 0 開始）</summary>
    public int StepOrder { get; set; }

    /// <summary>指令 ID（0~57）</summary>
    public int CommandId { get; set; }

    /// <summary>CRC8 校驗碼</summary>
    public int Crc { get; set; }

    /// <summary>參數0</summary>
    public double Arg0 { get; set; }

    /// <summary>參數1</summary>
    public double Arg1 { get; set; }

    /// <summary>參數2</summary>
    public double Arg2 { get; set; }

    /// <summary>參數3</summary>
    public double Arg3 { get; set; }

    /// <summary>參數4</summary>
    public double Arg4 { get; set; }

    /// <summary>字串參數（僅 CommandId=22,31,32,33 使用）</summary>
    public string? StringArg { get; set; }

    /// <summary>所屬群組名稱（對應 ## 標記），NULL=不屬於群組</summary>
    public string? GroupName { get; set; }

    /// <summary>群組巢狀深度</summary>
    public int GroupDepth { get; set; }

    /// <summary>導航屬性：所屬流程定義</summary>
    public FlowDefinition FlowDefinition { get; set; } = null!;
}
