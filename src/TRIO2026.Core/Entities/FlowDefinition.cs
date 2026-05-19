namespace TRIO2026.Core.Entities;

/// <summary>
/// 流程定義表，取代 flowlist.ini。
/// 對應資料庫: trio240plus_main.db
/// </summary>
public class FlowDefinition
{
    /// <summary>主鍵</summary>
    public int Id { get; set; }

    /// <summary>流程名稱（如 "P0001"）</summary>
    public string FlowName { get; set; } = string.Empty;

    /// <summary>流程說明</summary>
    public string? Description { get; set; }

    /// <summary>步驟總數</summary>
    public int TotalSteps { get; set; }

    /// <summary>版本號</summary>
    public string? Version { get; set; }

    /// <summary>樣本類型（FFPE-DNA, cfDNA 等）</summary>
    public string? SampleType { get; set; }

    /// <summary>是否啟用：0=停用, 1=啟用</summary>
    public int IsActive { get; set; } = 1;

    /// <summary>建立時間</summary>
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>修改時間</summary>
    public string ModifiedAt { get; set; } = string.Empty;

    /// <summary>修改者</summary>
    public string? ModifiedBy { get; set; }

    /// <summary>導航屬性：流程步驟集合</summary>
    public ICollection<FlowStep> Steps { get; set; } = new List<FlowStep>();
}
