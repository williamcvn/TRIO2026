namespace TRIO2026.Core.Entities;

/// <summary>
/// 檢測運行記錄表，每次實驗運行一筆。
/// 對應資料庫: data.db
/// 
/// 包含：
///   - 實驗參數（萃取程式、試劑批號、體積設定等）
///   - 操作員審計資訊（帳號、角色）
///   - 設備與軟體版本
///   - 運行狀態與錯誤追蹤
/// 
/// 製作者: Office of William
/// </summary>
public class TestRecord
{
    /// <summary>主鍵</summary>
    public int Id { get; set; }

    /// <summary>運行批次 ID（時間戳生成，唯一）</summary>
    public string RunId { get; set; } = string.Empty;

    // ── 報告類型 ──

    /// <summary>報告類型：IntelliPlex / Custom</summary>
    public string? ReportType { get; set; }

    /// <summary>執行的流程名稱</summary>
    public string FlowName { get; set; } = string.Empty;

    /// <summary>產品編碼</summary>
    public string? ProductCode { get; set; }

    // ── 操作員審計資訊 ──

    /// <summary>操作員 User.Id（FK 概念，不設實體 FK 因跨 DB）</summary>
    public int? OperatorUserId { get; set; }

    /// <summary>操作員帳號（快照，即使帳號日後刪除也保留記錄）</summary>
    public string? OperatorUsername { get; set; }

    /// <summary>操作員顯示名稱（快照）</summary>
    public string? OperatorDisplayName { get; set; }

    /// <summary>操作時的角色等級（1=Operator, 2=Service, 3=Admin）</summary>
    public int? RoleLevel { get; set; }

    // ── 設備資訊 ──

    /// <summary>設備序號</summary>
    public string? DeviceSerialNo { get; set; }

    /// <summary>軟體版本</summary>
    public string? SoftwareVersion { get; set; }

    // ── 實驗參數（對應 Excel Header 區域） ──

    /// <summary>實驗日期（yyyy/MM/dd）</summary>
    public string? ExperimentDate { get; set; }

    /// <summary>選取的功能模組（Custom Report 專用，如 Extraction）</summary>
    public string? FunctionModulesSelected { get; set; }

    /// <summary>萃取程式名稱</summary>
    public string? ExtractionProgram { get; set; }

    /// <summary>萃取試劑盒批號</summary>
    public string? ExtractionKitLotNo { get; set; }

    /// <summary>萃取樣本體積</summary>
    public string? ExtractionSampleVolume { get; set; }

    /// <summary>洗脫體積</summary>
    public string? ElutionVolume { get; set; }

    /// <summary>PCR 板 ID</summary>
    public string? PcrPlateId { get; set; }

    /// <summary>PCR 核酸輸入量（IntelliPlex 單值）</summary>
    public string? PcrTotalNucleicAcidInput { get; set; }

    /// <summary>IntelliPlex Kit 1 產品名稱</summary>
    public string? IntelliPlexKit1Name { get; set; }

    /// <summary>IntelliPlex Kit 1 批號</summary>
    public string? IntelliPlexKit1LotNo { get; set; }

    /// <summary>IntelliPlex Kit 2 產品名稱</summary>
    public string? IntelliPlexKit2Name { get; set; }

    /// <summary>IntelliPlex Kit 2 批號</summary>
    public string? IntelliPlexKit2LotNo { get; set; }

    /// <summary>
    /// 自訂 PCR 設定（Custom Report 專用）
    /// JSON 格式，包含各 Rxn 的 Control Assignment、Nucleic Acid Input、
    /// Sample Volume、Master Mix Volume
    /// </summary>
    public string? CustomPcrSetupJson { get; set; }

    /// <summary>S1 A/D 感測器值</summary>
    public string? S1AdValue { get; set; }

    /// <summary>S2 A/D 感測器值</summary>
    public string? S2AdValue { get; set; }

    // ── 運行資訊 ──

    /// <summary>樣本數量</summary>
    public int? SampleCount { get; set; }

    /// <summary>開始時間（ISO 8601）</summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>結束時間（ISO 8601）</summary>
    public string? EndTime { get; set; }

    /// <summary>狀態：Running / Completed / Error / Aborted</summary>
    public string Status { get; set; } = "Running";

    /// <summary>錯誤碼</summary>
    public string? ErrorCode { get; set; }

    /// <summary>錯誤訊息</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>備註</summary>
    public string? Notes { get; set; }

    // ── 導航屬性 ──

    /// <summary>導航屬性：樣本結果集合</summary>
    public ICollection<SampleResult> SampleResults { get; set; } = new List<SampleResult>();

    /// <summary>導航屬性：報表快照集合</summary>
    public ICollection<ReportSnapshot> ReportSnapshots { get; set; } = new List<ReportSnapshot>();
}
