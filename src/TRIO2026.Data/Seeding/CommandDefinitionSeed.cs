using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Seeding;

/// <summary>
/// 指令定義種子資料 — 來源：X_Flow_t 列舉 + funstep.ini 格式
/// 共 29 個指令（0~28），來自舊系統 commshowwidget.h 中的 X_Flow_t 列舉。
/// 
/// 參數類型 (ArgType): 0=無, 1=下拉選單, 2=數值輸入, 3=字串輸入
/// 參數選項 (ArgOptions): JSON 陣列，僅 ArgType=1 時有值
/// </summary>
public static class CommandDefinitionSeed
{
    public static List<CommandDefinition> GetSeedData()
    {
        return new List<CommandDefinition>
        {
            new() { Id = 0, Name = "空操作(NULL)", Note = "空指令，不執行任何動作" },

            new() { Id = 1, Name = "電機復位(MTREC)",
                Arg0Type = 1, Arg0Label = "PST軸", Arg0Options = "[\"不執行\",\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Arg1Type = 1, Arg1Label = "Z0軸", Arg1Options = "[\"不執行\",\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Arg2Type = 1, Arg2Label = "Y0軸", Arg2Options = "[\"不執行\",\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Arg3Type = 1, Arg3Label = "Y1軸", Arg3Options = "[\"不執行\",\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Arg4Type = 1, Arg4Label = "X0軸", Arg4Options = "[\"不執行\",\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Note = "多軸電機復位" },

            new() { Id = 2, Name = "延時(DELAY)",
                Arg0Type = 2, Arg0Label = "時間(S)",
                Note = "延時等待" },

            new() { Id = 3, Name = "移動PST到指定孔(PSTMOVE)",
                Arg0Type = 1, Arg0Label = "目標孔位", Arg0Options = "[\"H0\",\"H1\",\"H2\",\"H3\",\"H4\",\"H5\",\"H6\",\"H7\",\"H8\",\"H9\",\"H10\",\"H11\",\"H12\",\"H13\",\"H14\",\"H15\",\"W0\",\"W1\",\"W2\",\"W3\",\"W4\",\"W5\",\"W6\",\"W7\",\"C1\"]",
                Arg1Type = 2, Arg1Label = "磁吸距離(mm)",
                Arg2Type = 2, Arg2Label = "移動高度(mm)",
                Arg3Type = 1, Arg3Label = "移動速度", Arg3Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Arg4Type = 2, Arg4Label = "入位高度(mm)",
                Note = "移動活塞到指定孔位" },

            new() { Id = 4, Name = "設置活塞位置(PSTSET)",
                Arg0Type = 2, Arg0Label = "槍頭端高",
                Arg1Type = 2, Arg1Label = "活塞位置",
                Arg2Type = 1, Arg2Label = "移動速度", Arg2Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Note = "設置活塞的高度與位置" },

            new() { Id = 5, Name = "吸吐液(PSTSS)",
                Arg0Type = 2, Arg0Label = "吸吐體積(ul)",
                Arg1Type = 2, Arg1Label = "磁吸距離(mm)",
                Arg2Type = 2, Arg2Label = "吸吐高度(mm)",
                Arg3Type = 1, Arg3Label = "吸吐速度", Arg3Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Arg4Type = 2, Arg4Label = "前後延時(S)",
                Note = "活塞吸吐液操作" },

            new() { Id = 6, Name = "破孔(BROKEN)",
                Arg0Type = 1, Arg0Label = "開始孔位", Arg0Options = "[\"H0\",\"H1\",\"H2\",\"H3\",\"H4\",\"H5\",\"H6\",\"H7\",\"H8\",\"H9\",\"H10\",\"H11\",\"H12\",\"H13\",\"H14\",\"H15\",\"W0\",\"W1\",\"W2\",\"W3\",\"W4\",\"W5\",\"W6\",\"W7\",\"C1\"]",
                Arg1Type = 1, Arg1Label = "結束孔位", Arg1Options = "[\"H0\",\"H1\",\"H2\",\"H3\",\"H4\",\"H5\",\"H6\",\"H7\",\"H8\",\"H9\",\"H10\",\"H11\",\"H12\",\"H13\",\"H14\",\"H15\",\"W0\",\"W1\",\"W2\",\"W3\",\"W4\",\"W5\",\"W6\",\"W7\",\"C1\"]",
                Arg2Type = 2, Arg2Label = "移動高度(mm)",
                Arg3Type = 1, Arg3Label = "移動速度", Arg3Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Note = "破孔操作" },

            new() { Id = 7, Name = "電機偏移(MTOFFS)",
                Arg0Type = 1, Arg0Label = "電機名稱", Arg0Options = "[\"CH0\",\"CH1\",\"CH2\",\"CH3\",\"CH4\",\"CH5\",\"CH6\",\"CH7\",\"CH8:x0\",\"CH9:y0\",\"CH10:y1\",\"CH11:z0\",\"CH12:pst\",\"CH13\",\"CH14\",\"CH15\"]",
                Arg1Type = 2, Arg1Label = "偏移距離(mm)",
                Arg2Type = 1, Arg2Label = "偏移速度", Arg2Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Note = "單軸電機偏移" },

            new() { Id = 8, Name = "取退槍(RTTN)",
                Arg0Type = 1, Arg0Label = "槍頭類型", Arg0Options = "[\"空\",\"小槍頭\",\"大槍頭\"]",
                Arg1Type = 1, Arg1Label = "取拋操作", Arg1Options = "[\"拋棄操作\",\"拾取操作\"]",
                Arg2Type = 1, Arg2Label = "取拋位置", Arg2Options = "[\"H0\",\"H1\",\"H2\",\"H3\",\"H4\",\"H5\",\"H6\",\"H7\",\"H8\",\"H9\",\"H10\",\"H11\",\"H12\",\"H13\",\"H14\",\"H15\",\"W0\",\"W1\",\"W2\",\"W3\",\"W4\",\"W5\",\"W6\",\"W7\",\"C1\"]",
                Arg3Type = 1, Arg3Label = "移動速度", Arg3Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Note = "取退槍頭操作" },

            new() { Id = 9, Name = "加熱(HING)",
                Arg0Type = 1, Arg0Label = "加熱模組", Arg0Options = "[\"加熱器1\",\"加熱器2\",\"加熱器3\",\"加熱器4\"]",
                Arg1Type = 1, Arg1Label = "開關操作", Arg1Options = "[\"報告溫度\",\"開始加熱\",\"關閉加熱\"]",
                Arg2Type = 2, Arg2Label = "目標溫度(度)",
                Arg3Type = 1, Arg3Label = "等待模式", Arg3Options = "[\"不等待\",\"等到達溫\",\"定時關閉\"]",
                Arg4Type = 2, Arg4Label = "超時時間",
                Note = "加熱控制" },

            new() { Id = 10, Name = "輸出口控制(IOCTR)",
                Arg0Type = 1, Arg0Label = "操作模組", Arg0Options = "[\"排風扇\",\"UV燈\",\"門鎖\",\"照明\"]",
                Arg1Type = 1, Arg1Label = "開關操作", Arg1Options = "[\"關閉\",\"打開\"]",
                Arg2Type = 2, Arg2Label = "定時關閉(s)",
                Note = "IO輸出控制" },

            new() { Id = 11, Name = "迴圈開始(WHILE)",
                Arg0Type = 2, Arg0Label = "迴圈次數",
                Note = "迴圈開始標記" },

            new() { Id = 12, Name = "迴圈結束(WIEND)",
                Note = "迴圈結束標記" },

            new() { Id = 13, Name = "跟隨吸吐(FWXT)",
                Arg0Type = 2, Arg0Label = "吸吐體積(ul)",
                Arg1Type = 2, Arg1Label = "吸吐高度(mm)",
                Arg2Type = 1, Arg2Label = "吸吐速度", Arg2Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Arg3Type = 2, Arg3Label = "前後延時(S)",
                Note = "跟隨式吸吐液" },

            new() { Id = 14, Name = "輸入口判斷(INPUT)",
                Arg0Type = 1, Arg0Label = "輸入端口", Arg0Options = "[\"輸入1\",\"輸入2\",\"輸入3\",\"輸入4\"]",
                Arg1Type = 1, Arg1Label = "等待狀態", Arg1Options = "[\"低電平\",\"高電平\",\"低變高\",\"高變低\"]",
                Arg2Type = 2, Arg2Label = "等待時長(s)",
                Arg3Type = 2, Arg3Label = "失敗跳轉",
                Note = "GPIO輸入口狀態判斷" },

            new() { Id = 15, Name = "移動移液槍(PIPMV)",
                Arg0Type = 1, Arg0Label = "目標區域", Arg0Options = "[\"指定座標\",\"樣品槽\",\"槍頭盤\",\"試劑條\",\"96孔盤\",\"光學檢測\",\"緩衝管\",\"掃描條碼位置\",\"槍頭拋棄槽\"]",
                Arg1Type = 2, Arg1Label = "X座標(mm)",
                Arg2Type = 2, Arg2Label = "Y座標(mm)",
                Arg3Type = 2, Arg3Label = "移動高度(mm)",
                Arg4Type = 1, Arg4Label = "移動速度", Arg4Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Note = "移液槍XY移動" },

            new() { Id = 16, Name = "移液槍取退槍頭(PIPPT)",
                Arg0Type = 1, Arg0Label = "取退槍頭", Arg0Options = "[\"拋棄操作\",\"拾取操作\"]",
                Arg1Type = 1, Arg1Label = "取退速度", Arg1Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Note = "移液槍取退槍頭" },

            new() { Id = 17, Name = "移液槍吸吐液(PIPSS)",
                Arg0Type = 2, Arg0Label = "吸吐體積(ul)",
                Arg1Type = 1, Arg1Label = "追隨模式", Arg1Options = "[\"定點吸吐\",\"追隨模式\"]",
                Arg2Type = 1, Arg2Label = "吸吐速度", Arg2Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Arg3Type = 2, Arg3Label = "吸吐延時",
                Arg4Type = 2, Arg4Label = "過吸量",
                Note = "移液槍吸吐液" },

            new() { Id = 18, Name = "移液槍混打(PIPMX)",
                Arg0Type = 2, Arg0Label = "混打體積(ul)",
                Arg1Type = 2, Arg1Label = "混打次數",
                Arg2Type = 1, Arg2Label = "混打速度", Arg2Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Arg3Type = 2, Arg3Label = "吸吐延時",
                Arg4Type = 2, Arg4Label = "出位高度",
                Note = "移液槍混合打液" },

            new() { Id = 19, Name = "掃描條碼(SANQR)",
                Arg0Type = 1, Arg0Label = "開始位", Arg0Options = "[\"S1\",\"S2\",\"S3\",\"S4\",\"S5\",\"S6\",\"S7\",\"S8\",\"S9\",\"S10\",\"S11\",\"S12\",\"S13\",\"S14\",\"S15\",\"S16\",\"S17\",\"S18\",\"S19\",\"S20\",\"S21\",\"S22\",\"S23\",\"S24\"]",
                Arg1Type = 1, Arg1Label = "結束位", Arg1Options = "[\"S1\",\"S2\",\"S3\",\"S4\",\"S5\",\"S6\",\"S7\",\"S8\",\"S9\",\"S10\",\"S11\",\"S12\",\"S13\",\"S14\",\"S15\",\"S16\",\"S17\",\"S18\",\"S19\",\"S20\",\"S21\",\"S22\",\"S23\",\"S24\"]",
                Arg2Type = 2, Arg2Label = "掃碼高度",
                Arg3Type = 2, Arg3Label = "掃碼距離",
                Arg4Type = 1, Arg4Label = "運行速度", Arg4Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Note = "條碼掃描" },

            new() { Id = 20, Name = "移液槍模組復位(PIPRS)",
                Arg0Type = 1, Arg0Label = "活塞軸復位", Arg0Options = "[\"不執行\",\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Arg1Type = 1, Arg1Label = "PZ電機復位", Arg1Options = "[\"不執行\",\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Arg2Type = 1, Arg2Label = "PX電機復位", Arg2Options = "[\"不執行\",\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Arg3Type = 1, Arg3Label = "PY電機復位", Arg3Options = "[\"不執行\",\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Note = "移液槍多軸復位" },

            new() { Id = 21, Name = "執行並行流程(PARAL)",
                Arg0Type = 1, Arg0Label = "選擇程序", Arg0Options = "[\"PCR配液\",\"檢體加注\",\"放置物識別\",\"條碼識別\",\"光學檢測\"]",
                Note = "啟動並行子流程" },

            new() { Id = 22, Name = "光學倉開關蓋子(OPTFG)",
                Arg0Type = 1, Arg0Label = "開關蓋子", Arg0Options = "[\"關閉蓋子\",\"打開蓋子\",\"執行復位\"]",
                Note = "光學倉蓋控制" },

            new() { Id = 23, Name = "PCR換取新槍頭(PNEWPT)",
                Arg0Type = 1, Arg0Label = "槍頭類型", Arg0Options = "[\"丟棄槍頭\",\"50ul\",\"200ul\",\"1000ul\"]",
                Arg1Type = 1, Arg1Label = "移動速度", Arg1Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Note = "PCR模組換取新槍頭" },

            new() { Id = 24, Name = "PCR單吸單吐(PMVLIQ)",
                Arg0Type = 1, Arg0Label = "吸液位置", Arg0Options = "[\"樣本收集位\",\"試劑條位\",\"PCR盤位\",\"濃度檢測位\",\"稀釋液位\"]",
                Arg1Type = 2, Arg1Label = "孔位編號",
                Arg2Type = 1, Arg2Label = "吸吐方式", Arg2Options = "[\"定點吸吐液\",\"探測吸吐液\",\"定點吸探測吐\",\"探測吸定點吐\"]",
                Arg3Type = 2, Arg3Label = "高度配置",
                Arg4Type = 1, Arg4Label = "吸吐速度", Arg4Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Note = "PCR單吸單吐" },

            new() { Id = 25, Name = "PCR單吸多吐(PEQULIQ)",
                Arg0Type = 1, Arg0Label = "吸液位置", Arg0Options = "[\"樣本收集位\",\"試劑條位\",\"PCR盤位\",\"濃度檢測位\",\"稀釋液位\"]",
                Arg1Type = 2, Arg1Label = "孔位編號",
                Arg2Type = 1, Arg2Label = "吸吐方式", Arg2Options = "[\"定點吸吐液\",\"探測吸吐液\",\"定點吸探測吐\",\"探測吸定點吐\"]",
                Arg3Type = 2, Arg3Label = "高度配置",
                Arg4Type = 1, Arg4Label = "吸吐速度", Arg4Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Note = "PCR單吸多吐" },

            new() { Id = 26, Name = "PCR混液(PMIXLIQ)",
                Arg0Type = 1, Arg0Label = "吸液位置", Arg0Options = "[\"樣本收集位\",\"試劑條位\",\"PCR盤位\",\"濃度檢測位\",\"稀釋液位\"]",
                Arg1Type = 2, Arg1Label = "孔位編號",
                Arg2Type = 2, Arg2Label = "混打次數",
                Arg3Type = 2, Arg3Label = "混打高度",
                Arg4Type = 1, Arg4Label = "混打速度", Arg4Options = "[\"速度1\",\"速度2\",\"速度3\",\"速度4\",\"速度5\",\"速度6\",\"速度7\",\"速度8\",\"速度9\",\"速度10\"]",
                Note = "PCR混合打液" },

            new() { Id = 27, Name = "PCR濃度檢測(POPTDCT)",
                Arg0Type = 1, Arg0Label = "檢測類型", Arg0Options = "[\"硬體校準\",\"DNA標定\",\"DNA檢測\",\"RNA標定\",\"RNA檢測\"]",
                Arg1Type = 2, Arg1Label = "檢測孔位",
                Note = "PCR光學濃度檢測" },

            new() { Id = 28, Name = "流程結束(END)",
                Note = "流程結束標記，Id=0xFF在舊系統" },
        };
    }
}
