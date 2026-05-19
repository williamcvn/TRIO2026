namespace TRIO2026.Core.Entities;

/// <summary>
/// 產品流程映射表，取代 flowinfo.ini [Flow] Section。
/// 對應資料庫: trio240plus_main.db
/// 
/// 來源規格: A09-023_軟體設計規格書_VG_附件4_flowinfo_Flow table
/// 原始欄位: 流程代碼, 對應TRIO內建流程名稱, 洗脫體積, 上樣體積, 樣本種類, 耗材擺放代碼(A-B-C-D-E-F), 萃取時間
/// </summary>
public class FlowMapping
{
    /// <summary>主鍵（自動遞增）</summary>
    public int Id { get; set; }

    /// <summary>
    /// 流程代碼
    /// 原始中文: 流程代碼
    /// 範例: "P0001", "C0001", "NE001", "MT001", "QC001"
    /// 命名規則: P=產品流程, C=客製流程, NE=無萃取, MT=維護, QC=品管
    /// </summary>
    public string FlowCode { get; set; } = string.Empty;

    /// <summary>
    /// 對應 TRIO 內建流程名稱
    /// 原始中文: 對應TRIO內建流程名稱
    /// 範例: "P0001", "Dilute Progream 2 V11", "Installation Position Check V2"
    /// 說明: 此名稱對應 flowlist.ini 中的實際流程檔案名
    /// </summary>
    public string BuiltInFlowName { get; set; } = string.Empty;

    /// <summary>
    /// 洗脫體積（μL）
    /// 原始中文: 洗脫體積
    /// 範例: "60", "200", "N/A"
    /// 說明: DNA/RNA 洗脫步驟的體積，"N/A" 表示不適用（如無萃取流程）
    /// </summary>
    public string? ElutionVolume { get; set; }

    /// <summary>
    /// 上樣體積（μL）
    /// 原始中文: 上樣體積
    /// 範例: "5000", "10000", "400", "N/A"
    /// 說明: 樣品加入的體積，"N/A" 表示不適用
    /// </summary>
    public string? LoadingVolume { get; set; }

    /// <summary>
    /// 樣本種類
    /// 原始中文: 樣本種類
    /// 範例: "FFPE-DNA", "cfDNA", "FFPE-RNA", "virus//RNA", "gDNA", "RNA", "TEST"
    /// </summary>
    public string? SampleType { get; set; }

    /// <summary>
    /// 耗材擺放代碼（A-B-C-D-E-F 六段式）
    /// 原始中文: 耗材擺放代碼(A-B-C-D-E-F)
    /// 範例: "1-1-1-1-1-1", "0-5-1-1-1-1", "2-2-1-1-1-1"
    /// 
    /// 各段代碼意義:
    ///   A = 萃取模組配置
    ///   B = 稀釋程式版本
    ///   C = 標準品配置
    ///   D = 基本耗材配置
    ///   E = PCR 模組配置
    ///   F = 預留擴展位
    /// </summary>
    public string? ConsumableLayoutCode { get; set; }

    /// <summary>
    /// 萃取時間（秒）
    /// 原始中文: 萃取時間
    /// 範例: 8100 (=135分鐘), 9300, 0 (無萃取流程)
    /// 說明: 整個萃取流程的預估執行秒數
    /// </summary>
    public int? ExtractionTime { get; set; }
}
