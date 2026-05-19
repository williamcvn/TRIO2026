# TRIO2026 Seed Data 植入報告

> **文件編號**: TRIO2026-WL-002  
> **撰寫**: Office of William  
> **日期**: 2026-04-28 14:39  
> **任務範圍**: 四個 .db 的初始資料匯入

---

## 一、植入總覽

| 資料庫 | 表 | 筆數 | 資料來源 |
|--------|-----|-----:|---------|
| **config.db** | CommandDefinition | **29** | X_Flow_t 列舉 |
| **config.db** | SystemConfig | **2,497** | 12 個 config/*.ini |
| **main.db** | FlowDefinition | **10** | 10 個 .flow 檔案 |
| **main.db** | FlowStep | **2,091** | 流程步驟明細 |
| **main.db** | FlowMapping | **18** | flowinfo.ini [Flow] |
| **main.db** | PnidMapping | **24** | flowinfo.ini [PNID] |
| **main.db** | UserAccount | **3** | 預設帳號 |
| **data.db** | (3 表) | **0** | 運行時產生 |
| **log.db** | (2 表) | **0** | 運行時產生 |

### **總計: 4,672 筆 Seed Data**

---

## 二、SystemConfig 分類明細（2,497 筆）

| Category | 筆數 | 原始檔案 | 說明 |
|----------|-----:|---------|------|
| area_position | 1,314 | areaposcfg.ini | 16 區座標位置（含 96 孔盤） |
| motor | 397 | motocfg.ini | 16 軸電機參數 |
| pipette | 369 | pipetteinfo.ini | 8 種移液槍頭校正 |
| flow_info | 125 | flowinfo.ini | Flow/PNID 映射 |
| tube | 106 | tubecfg.ini | 13 種管型尺寸 |
| optics | 83 | opticsinfo.ini | 光學校正表 |
| trio_info | 56 | trioinfo.ini | 孔位偏移/活塞參數 |
| flow_list | 26 | flowlist.ini | 流程名稱索引 |
| camera | 10 | cameracfg.ini | 攝像頭區域座標 |
| temperature | 8 | temperaturecfg.ini | 加熱器配置 |
| maintenance | 2 | maintenance.ini | 拆箱維護位置 |
| system | 1 | syscfg.ini | 功能模式旗標 |

---

## 三、FlowDefinition 明細（10 定義 + 2,091 步驟）

| 流程名稱 | 步驟數 | 來源檔案 |
|----------|-----:|---------|
| Dilute Progream 4 V1 | 370 | Dilute Progream 4 V1.flow |
| Dilution Program 4 V1 | 321 | Dilution Program 4 V1.flow |
| Dilution Program 4 V2 | 321 | Dilution Program 4 V2.flow |
| Dilution Program 4 V4 | 320 | Dilution Program 4 V4.flow |
| Dilution Program 4 | 322 | Dilution Program 4 V5(Cora).flow |
| Dilution Program 4_5 | 323 | Dilution Program 4.flow |
| Opti_2 | 102 | Opti_2.flow |
| Test1 | 2 | Test1.flow |
| Test2 | 4 | Test2.flow |
| Test3 | 6 | Test3.flow |

---

## 四、CommandDefinition（29 筆指令）

| Id | 指令名稱 | 參數概述 |
|----|---------|---------|
| 0 | 空操作(NULL) | 無參數 |
| 1 | 電機復位(MTREC) | 5 軸速度選擇 |
| 2 | 延時(DELAY) | 時間(S) |
| 3 | 移動PST到指定孔(PSTMOVE) | 孔位/距離/高度/速度 |
| 4 | 設置活塞位置(PSTSET) | 端高/位置/速度 |
| 5 | 吸吐液(PSTSS) | 體積/距離/高度/速度/延時 |
| 6 | 破孔(BROKEN) | 起止孔位/高度/速度 |
| 7 | 電機偏移(MTOFFS) | 電機/距離/速度 |
| 8 | 取退槍(RTTN) | 類型/操作/位置/速度 |
| 9 | 加熱(HING) | 模組/操作/溫度/等待/超時 |
| 10 | 輸出口控制(IOCTR) | 模組/操作/定時 |
| 11 | 迴圈開始(WHILE) | 次數 |
| 12 | 迴圈結束(WIEND) | 無 |
| 13 | 跟隨吸吐(FWXT) | 體積/高度/速度/延時 |
| 14 | 輸入口判斷(INPUT) | 端口/狀態/時長/跳轉 |
| 15 | 移動移液槍(PIPMV) | 區域/XY座標/高度/速度 |
| 16 | 取退槍頭(PIPPT) | 操作/速度 |
| 17 | 移液槍吸吐(PIPSS) | 體積/追隨/速度/延時/過吸 |
| 18 | 移液槍混打(PIPMX) | 體積/次數/速度/延時/高度 |
| 19 | 掃描條碼(SANQR) | 起止位/高度/距離/速度 |
| 20 | 移液槍復位(PIPRS) | 4 軸速度選擇 |
| 21 | 並行流程(PARAL) | 程序選擇 |
| 22 | 光學倉蓋(OPTFG) | 開關/復位 |
| 23 | PCR換槍頭(PNEWPT) | 類型/速度 |
| 24 | PCR單吸單吐(PMVLIQ) | 位置/孔位/方式/高度/速度 |
| 25 | PCR單吸多吐(PEQULIQ) | 位置/孔位/方式/高度/速度 |
| 26 | PCR混液(PMIXLIQ) | 位置/孔位/次數/高度/速度 |
| 27 | PCR濃度檢測(POPTDCT) | 類型/孔位 |
| 28 | 流程結束(END) | 無 |

---

## 五、UserAccount（3 筆預設帳號）

| Username | 角色 | RoleLevel | 備註 |
|----------|------|-----------|------|
| admin | 管理員 | 3 | PLACEHOLDER 密碼，部署時需替換 |
| service | Service 工程師 | 2 | PLACEHOLDER 密碼，部署時需替換 |
| operator | 操作員 | 1 | PLACEHOLDER 密碼，部署時需替換 |

---

## 六、植入工具與執行順序

```bash
# 1. 建表 + C# Seed Data
dotnet run --project tools/DbInitializer

# 2. 匯入 12 個 INI → SystemConfig
python tools/import_system_config.py

# 3. 匯入 .flow → FlowDefinition + FlowStep
python tools/import_flow_definitions.py

# 4. 驗證
python tools/verify_schema.py
```

### 冪等性設計
- **C# Seed**: `AnyAsync()` 判斷，已存在則跳過
- **Python**: `SELECT COUNT(*)` 判斷，已存在則跳過

---

*文件結束*
