using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Seeding;

/// <summary>
/// FlowMapping 和 PnidMapping 種子資料
/// 
/// FlowMapping 來源: A09-023_軟體設計規格書_VG_附件4_flowinfo_Flow table
/// 原始欄位: 流程代碼, 對應TRIO內建流程名稱, 洗脫體積, 上樣體積, 樣本種類, 耗材擺放代碼(A-B-C-D-E-F), 萃取時間
/// 
/// PnidMapping 來源: config/flowinfo.ini [PNID] Section
/// </summary>
public static class FlowInfoSeed
{
    /// <summary>
    /// 22 筆 FlowMapping — 來自 A09-023 附件4 (最新版)
    /// </summary>
    public static List<FlowMapping> GetFlowMappings()
    {
        return new List<FlowMapping>
        {
            // === P 系列: 產品流程 ===
            new() { Id = 1,  FlowCode = "P0001", BuiltInFlowName = "P0001",                     ElutionVolume = "60",  LoadingVolume = "N/A",   SampleType = "FFPE-DNA",   ConsumableLayoutCode = "1-1-1-1-1-1", ExtractionTime = 8100 },
            new() { Id = 2,  FlowCode = "P0002", BuiltInFlowName = "P0002",                     ElutionVolume = "60",  LoadingVolume = "5000",  SampleType = "cfDNA",      ConsumableLayoutCode = "1-3-1-1-1-1", ExtractionTime = 9300 },
            new() { Id = 3,  FlowCode = "P0003", BuiltInFlowName = "P0003",                     ElutionVolume = "60",  LoadingVolume = "N/A",   SampleType = "FFPE-RNA",   ConsumableLayoutCode = "1-6-1-1-2-1", ExtractionTime = 8400 },
            new() { Id = 4,  FlowCode = "P0008", BuiltInFlowName = "P0008",                     ElutionVolume = "60",  LoadingVolume = "10000", SampleType = "cfDNA",      ConsumableLayoutCode = "2-2-1-1-1-1", ExtractionTime = 10800 },

            // === C 系列: 客製流程 ===
            new() { Id = 5,  FlowCode = "C0001", BuiltInFlowName = "C0001",                     ElutionVolume = "60",  LoadingVolume = "400",   SampleType = "virus//RNA", ConsumableLayoutCode = "1-4-1-1-3-1", ExtractionTime = 2400 },
            new() { Id = 6,  FlowCode = "C0002", BuiltInFlowName = "C0002",                     ElutionVolume = "60",  LoadingVolume = "N/A",   SampleType = "FFPE-DNA",   ConsumableLayoutCode = "1-1-1-1-3-1", ExtractionTime = 8100 },
            new() { Id = 7,  FlowCode = "C0003", BuiltInFlowName = "C0003",                     ElutionVolume = "60",  LoadingVolume = "N/A",   SampleType = "FFPE-RNA",   ConsumableLayoutCode = "1-6-1-1-3-1", ExtractionTime = 8400 },
            new() { Id = 8,  FlowCode = "C0004", BuiltInFlowName = "C0004",                     ElutionVolume = "60",  LoadingVolume = "N/A",   SampleType = "FFPE-DNA",   ConsumableLayoutCode = "1-7-1-1-3-1", ExtractionTime = 12000 },
            new() { Id = 9,  FlowCode = "C0005", BuiltInFlowName = "C0005",                     ElutionVolume = "60",  LoadingVolume = "5000",  SampleType = "cfDNA",      ConsumableLayoutCode = "1-3-1-1-3-1", ExtractionTime = 9300 },
            new() { Id = 10, FlowCode = "C0006", BuiltInFlowName = "C0006",                     ElutionVolume = "200", LoadingVolume = "200",   SampleType = "gDNA",       ConsumableLayoutCode = "1-4-1-1-3-1", ExtractionTime = 3600 },
            new() { Id = 11, FlowCode = "C0007", BuiltInFlowName = "C0007",                     ElutionVolume = "60",  LoadingVolume = "400",   SampleType = "RNA",        ConsumableLayoutCode = "1-4-1-1-3-1", ExtractionTime = 5700 },
            new() { Id = 12, FlowCode = "C0008", BuiltInFlowName = "C0008",                     ElutionVolume = "60",  LoadingVolume = "10000", SampleType = "cfDNA",      ConsumableLayoutCode = "2-2-1-1-3-1", ExtractionTime = 10800 },

            // === NE 系列: 無萃取流程 (No Extraction) ===
            new() { Id = 13, FlowCode = "NE001", BuiltInFlowName = "Dilute Progream 2 V11",     ElutionVolume = "N/A", LoadingVolume = "N/A",   SampleType = "FFPE-DNA",   ConsumableLayoutCode = "0-5-1-1-1-1", ExtractionTime = 0 },
            new() { Id = 14, FlowCode = "NE002", BuiltInFlowName = "Dilute Progream 2 V11",     ElutionVolume = "N/A", LoadingVolume = "N/A",   SampleType = "cfDNA",      ConsumableLayoutCode = "0-5-1-1-1-1", ExtractionTime = 0 },
            new() { Id = 15, FlowCode = "NE003", BuiltInFlowName = "Dilute Progream 2 V11_RNA", ElutionVolume = "N/A", LoadingVolume = "N/A",   SampleType = "FFPE-RNA",   ConsumableLayoutCode = "0-5-0-1-2-1", ExtractionTime = 0 },

            // === MT 系列: 維護流程 (Maintenance) ===
            new() { Id = 16, FlowCode = "MT001", BuiltInFlowName = "Installation Position Check V2", ElutionVolume = "N/A", LoadingVolume = "N/A", SampleType = "TEST", ConsumableLayoutCode = "0-0-0-1-0-0", ExtractionTime = 0 },
            new() { Id = 17, FlowCode = "MT002", BuiltInFlowName = "Dilute Progream 2 V11_CFS", ElutionVolume = "N/A", LoadingVolume = "N/A",   SampleType = "TEST",       ConsumableLayoutCode = "0-5-0-1-1-1", ExtractionTime = 0 },
            new() { Id = 18, FlowCode = "MT003", BuiltInFlowName = "Dilute Progream 2 V11_CFS", ElutionVolume = "N/A", LoadingVolume = "N/A",   SampleType = "TEST",       ConsumableLayoutCode = "0-5-1-1-0-0", ExtractionTime = 0 },

            // === QC 系列: 品管流程 (Quality Control) ===
            new() { Id = 19, FlowCode = "QC001", BuiltInFlowName = "Dilute Progream 2 V11_QC",  ElutionVolume = "N/A", LoadingVolume = "N/A",   SampleType = "TEST",       ConsumableLayoutCode = "0-5-1-1-1-1", ExtractionTime = 0 },
            new() { Id = 20, FlowCode = "QC002", BuiltInFlowName = "Dilute Progream 2 V11_QC",  ElutionVolume = "N/A", LoadingVolume = "N/A",   SampleType = "TEST",       ConsumableLayoutCode = "0-5-1-1-1-1", ExtractionTime = 0 },
            new() { Id = 21, FlowCode = "QC003", BuiltInFlowName = "S1101-4000-60-hole",        ElutionVolume = "N/A", LoadingVolume = "N/A",   SampleType = "TEST",       ConsumableLayoutCode = "1-3-0-0-0-0", ExtractionTime = 9300 },
            new() { Id = 22, FlowCode = "QC004", BuiltInFlowName = "Dilute Progream 2 V11_QC",  ElutionVolume = "N/A", LoadingVolume = "N/A",   SampleType = "TEST",       ConsumableLayoutCode = "0-5-1-1-0-0", ExtractionTime = 0 },
        };
    }

    /// <summary>
    /// 24 筆 PnidMapping — 來自 flowinfo.ini [PNID] Section
    /// 格式: "PnidCode,DescriptionEn,DescriptionZh"
    /// </summary>
    public static List<PnidMapping> GetPnidMappings()
    {
        return new List<PnidMapping>
        {
            new() { Id = 1,  PnidCode = "82004", DescriptionEn = null,  DescriptionZh = "82004 BRAF V600" },
            new() { Id = 2,  PnidCode = "82020", DescriptionEn = null,  DescriptionZh = "82020 NRAS" },
            new() { Id = 3,  PnidCode = "82022", DescriptionEn = null,  DescriptionZh = "82022 KRAS" },
            new() { Id = 4,  PnidCode = "82021", DescriptionEn = null,  DescriptionZh = "82021 PIK3CA" },
            new() { Id = 5,  PnidCode = "82032", DescriptionEn = null,  DescriptionZh = "82032 Lung Cancer Panel-DNA" },
            new() { Id = 6,  PnidCode = "82030", DescriptionEn = null,  DescriptionZh = "82030 Lung Cancer Panel-cfDNA" },
            new() { Id = 7,  PnidCode = "82023", DescriptionEn = null,  DescriptionZh = "82023 ALK Rearrangement" },
            new() { Id = 8,  PnidCode = "82024", DescriptionEn = null,  DescriptionZh = "82024 ROS1 Rearrangement" },
            new() { Id = 9,  PnidCode = "82025", DescriptionEn = null,  DescriptionZh = "82025 RET/NTRK1 Rearrangement" },
            new() { Id = 10, PnidCode = "82033", DescriptionEn = null,  DescriptionZh = "82033 Lung Cancer Panel-RNA" },
            new() { Id = 11, PnidCode = "83015", DescriptionEn = "83015 FFPE DNA", DescriptionZh = null },
            new() { Id = 12, PnidCode = "83016", DescriptionEn = "83016 FFPE RNA", DescriptionZh = null },
            new() { Id = 13, PnidCode = "83013", DescriptionEn = "83013 cfDNA(<=5mL)", DescriptionZh = null },
            new() { Id = 14, PnidCode = "83028", DescriptionEn = "83028 cfDNA(<=10mL)", DescriptionZh = null },
            new() { Id = 15, PnidCode = "83012", DescriptionEn = "83012 Viral DNA/RNA", DescriptionZh = null },
            new() { Id = 16, PnidCode = "83029", DescriptionEn = "83029 FFPE DNA Plus", DescriptionZh = null },
            new() { Id = 17, PnidCode = "83026", DescriptionEn = "83026 Blood gDNA", DescriptionZh = null },
            new() { Id = 18, PnidCode = "83027", DescriptionEn = "83027 Blood total RNA", DescriptionZh = null },
            new() { Id = 19, PnidCode = "NE001", DescriptionEn = "Workflow without Extraction", DescriptionZh = null },
            new() { Id = 20, PnidCode = "NE002", DescriptionEn = "Workflow without Extraction", DescriptionZh = null },
            new() { Id = 21, PnidCode = "NE003", DescriptionEn = "Workflow without Extraction", DescriptionZh = null },
            new() { Id = 22, PnidCode = "MT001", DescriptionEn = "Installation Position Check", DescriptionZh = "Installation Position Check" },
            new() { Id = 23, PnidCode = "QC001", DescriptionEn = "QC Dilution Test_0.5ng/ul", DescriptionZh = "QC Dilution Test_0.5ng/ul" },
            new() { Id = 24, PnidCode = "QC002", DescriptionEn = "QC Dilution Test_2.5ng/ul", DescriptionZh = "QC Dilution Test_2.5ng/ul" },
        };
    }
}
