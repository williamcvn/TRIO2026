# TRIO2026 資料字典

> **文件編號**: TRIO2026-DB-002  
> **撰寫**: Office of William  
> **日期**: 2026-04-28  
> **版本**: 1.1（FlowMapping 欄位修正）  
> **說明**: 四個資料庫共 12 張表的完整欄位定義  
> **參考規格**: A09-023_軟體設計規格書_VG_附件4_flowinfo_Flow table  

---

## 一、trio240plus_config.db（機器配置庫）

### 1.1 SystemConfig — 通用配置表

取代舊系統 12 個 .ini 檔案的 Key-Value 配置。  
**目前資料量**: 2,497 筆  
**C# Entity**: `TRIO2026.Core.Entities.SystemConfig`

| # | 欄位名稱 | 資料型別 | NOT NULL | 預設值 | PK | 說明 |
|---|---------|---------|:--------:|--------|:--:|------|
| 0 | Id | INTEGER | ✓ | (自動) | ✓ | 主鍵（自動遞增） |
| 1 | Category | TEXT | ✓ | — | | 配置分類（motor, area_position, temperature 等） |
| 2 | Key | TEXT | ✓ | — | | 參數名稱，格式: `[Section].key` |
| 3 | Value | TEXT | | — | | 參數值 |
| 4 | DataType | TEXT | ✓ | 'string' | | 值的型別提示: int / float / string / bool |
| 5 | Description | TEXT | | — | | 參數說明 |
| 6 | ModifiedAt | TEXT | ✓ | — | | 修改時間（ISO8601） |
| 7 | ModifiedBy | TEXT | | — | | 修改者 |

**索引**:
| 索引名稱 | 欄位 | 唯一 |
|---------|------|:----:|
| IX_SystemConfig_Category_Key | Category, Key | ✓ |

**Category 值域（12 種）**:

| Category | 對應舊檔案 | 筆數 | 說明 |
|----------|-----------|-----:|------|
| area_position | areaposcfg.ini | 1,314 | 16 個區域的座標位置 |
| motor | motocfg.ini | 397 | 16 軸電機參數 |
| pipette | pipetteinfo.ini | 369 | 8 種移液槍頭校正 |
| flow_info | flowinfo.ini | 125 | 樣本類型/執行時間 |
| tube | tubecfg.ini | 106 | 13 種管型尺寸 |
| optics | opticsinfo.ini | 83 | 光學校正表 |
| trio_info | trioinfo.ini | 56 | 孔位偏移量/活塞參數 |
| flow_list | flowlist.ini | 26 | 流程名稱索引 |
| camera | cameracfg.ini | 10 | 攝像頭區域座標 |
| temperature | temperaturecfg.ini | 8 | 加熱器配置 |
| maintenance | maintenance.ini | 2 | 拆箱維護位置 |
| system | syscfg.ini | 1 | 功能模式旗標 |

---

### 1.2 CommandDefinition — 指令定義表

定義 29 個流程控制指令，每個指令可帶 5 組參數。  
**目前資料量**: 29 筆（Id: 0~28）  
**C# Entity**: `TRIO2026.Core.Entities.CommandDefinition`

| # | 欄位名稱 | 資料型別 | NOT NULL | 預設值 | PK | 說明 |
|---|---------|---------|:--------:|--------|:--:|------|
| 0 | Id | INTEGER | ✓ | (手動) | ✓ | 指令 ID（0~28），手動指定 |
| 1 | Name | TEXT | ✓ | — | | 指令名稱 |
| 2 | Arg0Type | INTEGER | ✓ | 0 | | 參數0類型: 0=無, 1=下拉, 2=數值, 3=字串 |
| 3 | Arg0Label | TEXT | | — | | 參數0標籤 |
| 4 | Arg0Options | TEXT | | — | | 參數0選項（JSON 陣列） |
| 5 | Arg1Type | INTEGER | ✓ | 0 | | 參數1類型 |
| 6 | Arg1Label | TEXT | | — | | 參數1標籤 |
| 7 | Arg1Options | TEXT | | — | | 參數1選項 |
| 8 | Arg2Type | INTEGER | ✓ | 0 | | 參數2類型 |
| 9 | Arg2Label | TEXT | | — | | 參數2標籤 |
| 10 | Arg2Options | TEXT | | — | | 參數2選項 |
| 11 | Arg3Type | INTEGER | ✓ | 0 | | 參數3類型 |
| 12 | Arg3Label | TEXT | | — | | 參數3標籤 |
| 13 | Arg3Options | TEXT | | — | | 參數3選項 |
| 14 | Arg4Type | INTEGER | ✓ | 0 | | 參數4類型 |
| 15 | Arg4Label | TEXT | | — | | 參數4標籤 |
| 16 | Arg4Options | TEXT | | — | | 參數4選項 |
| 17 | Note | TEXT | | — | | 指令說明 |
| 18 | DisplayFormat | TEXT | | — | | 顯示格式模板 |

> **參數類型（ArgType）說明**:  
> `0` = 無此參數（UI 隱藏）  
> `1` = 下拉選單（ArgOptions 為 JSON 陣列）  
> `2` = 數值輸入（可輸入整數/浮點/十六進位）  
> `3` = 字串輸入

---

## 二、trio240plus_main.db（業務核心庫）

### 2.1 UserAccount — 使用者帳號表

**目前資料量**: 3 筆  
**C# Entity**: `TRIO2026.Core.Entities.UserAccount`

| # | 欄位名稱 | 資料型別 | NOT NULL | 預設值 | PK | 說明 |
|---|---------|---------|:--------:|--------|:--:|------|
| 0 | Id | INTEGER | ✓ | (自動) | ✓ | 主鍵 |
| 1 | Username | TEXT | ✓ | — | | 使用者帳號 |
| 2 | PasswordHash | TEXT | ✓ | — | | BCrypt 密碼雜湊（禁止明碼） |
| 3 | RoleLevel | INTEGER | ✓ | 1 | | 角色等級: 1=操作員, 2=Service, 3=管理員 |
| 4 | IsActive | INTEGER | ✓ | 1 | | 帳號狀態: 0=停用, 1=啟用 |
| 5 | CreatedAt | TEXT | ✓ | — | | 建立時間（ISO8601） |
| 6 | LastLoginAt | TEXT | | — | | 最後登入時間 |

**索引**:
| 索引名稱 | 欄位 | 唯一 |
|---------|------|:----:|
| IX_UserAccount_Username | Username | ✓ |

**RoleLevel 值域**:

| 值 | 角色 | 權限說明 |
|---|------|---------|
| 1 | 操作員 (Operator) | 基本操作 |
| 2 | Service 工程師 | 系統設定 + 進階維護 |
| 3 | 管理員 (Admin) | 全部權限 |

---

### 2.2 FlowDefinition — 流程定義表

**目前資料量**: 10 筆  
**C# Entity**: `TRIO2026.Core.Entities.FlowDefinition`

| # | 欄位名稱 | 資料型別 | NOT NULL | 預設值 | PK | 說明 |
|---|---------|---------|:--------:|--------|:--:|------|
| 0 | Id | INTEGER | ✓ | (自動) | ✓ | 主鍵 |
| 1 | FlowName | TEXT | ✓ | — | | 流程名稱（唯一） |
| 2 | Description | TEXT | | — | | 流程說明 |
| 3 | TotalSteps | INTEGER | ✓ | 0 | | 步驟總數 |
| 4 | Version | TEXT | | — | | 版本號 |
| 5 | SampleType | TEXT | | — | | 樣本類型 |
| 6 | IsActive | INTEGER | ✓ | 1 | | 是否啟用 |
| 7 | CreatedAt | TEXT | ✓ | — | | 建立時間 |
| 8 | ModifiedAt | TEXT | ✓ | — | | 修改時間 |
| 9 | ModifiedBy | TEXT | | — | | 修改者 |

**索引**:
| 索引名稱 | 欄位 | 唯一 |
|---------|------|:----:|
| IX_FlowDefinition_FlowName | FlowName | ✓ |

---

### 2.3 FlowStep — 流程步驟表

**目前資料量**: 2,091 筆  
**C# Entity**: `TRIO2026.Core.Entities.FlowStep`

| # | 欄位名稱 | 資料型別 | NOT NULL | 預設值 | PK | 說明 |
|---|---------|---------|:--------:|--------|:--:|------|
| 0 | Id | INTEGER | ✓ | (自動) | ✓ | 主鍵 |
| 1 | FlowDefinitionId | INTEGER | ✓ | — | | FK → FlowDefinition.Id |
| 2 | StepOrder | INTEGER | ✓ | — | | 步驟序號（從 0 開始） |
| 3 | CommandId | INTEGER | ✓ | — | | 指令 ID（0~28） |
| 4 | Crc | INTEGER | ✓ | 0 | | CRC8 校驗碼 |
| 5 | Arg0 | REAL | ✓ | 0.0 | | 參數0 |
| 6 | Arg1 | REAL | ✓ | 0.0 | | 參數1 |
| 7 | Arg2 | REAL | ✓ | 0.0 | | 參數2 |
| 8 | Arg3 | REAL | ✓ | 0.0 | | 參數3 |
| 9 | Arg4 | REAL | ✓ | 0.0 | | 參數4 |
| 10 | StringArg | TEXT | | — | | 字串參數（CommandId=22,31,32,33） |
| 11 | GroupName | TEXT | | — | | 所屬群組名稱 |
| 12 | GroupDepth | INTEGER | ✓ | 0 | | 群組巢狀深度 |

**索引**:
| 索引名稱 | 欄位 | 唯一 |
|---------|------|:----:|
| IX_FlowStep_FlowDefinitionId_StepOrder | FlowDefinitionId, StepOrder | |

**外鍵**:
| 來源欄位 | 目標表.欄位 | ON DELETE |
|---------|-----------|----------|
| FlowDefinitionId | FlowDefinition.Id | CASCADE |

---

### 2.4 FlowMapping — 產品流程映射表

取代 `flowinfo.ini` 的 `[Flow]` Section。  
**來源規格**: `A09-023_軟體設計規格書_VG_附件4_flowinfo_Flow table`  
**原始欄位定義**: 流程代碼, 對應TRIO內建流程名稱, 洗脫體積, 上樣體積, 樣本種類, 耗材擺放代碼(A-B-C-D-E-F), 萃取時間  
**目前資料量**: 22 筆  
**C# Entity**: `TRIO2026.Core.Entities.FlowMapping`

| # | 欄位名稱 | 資料型別 | NOT NULL | PK | 原始中文 | 說明 | 範例值 |
|---|---------|---------|:--------:|:--:|---------|------|-------|
| 0 | Id | INTEGER | ✓ | ✓ | — | 主鍵（自動遞增） | 1, 2, ... |
| 1 | FlowCode | TEXT | ✓ | | 流程代碼 | 唯一流程識別碼。P=產品, C=客製, NE=無萃取, MT=維護, QC=品管 | `"P0001"`, `"C0001"`, `"NE001"`, `"MT001"`, `"QC001"` |
| 2 | BuiltInFlowName | TEXT | ✓ | | 對應TRIO內建流程名稱 | 對應 flowlist.ini 中的實際流程檔案名 | `"P0001"`, `"Dilute Progream 2 V11"` |
| 3 | ElutionVolume | TEXT | | | 洗脫體積 | DNA/RNA 洗脫步驟的體積（μL），`"N/A"` 表不適用 | `"60"`, `"200"`, `"N/A"` |
| 4 | LoadingVolume | TEXT | | | 上樣體積 | 樣品加入體積（μL），`"N/A"` 表不適用 | `"5000"`, `"10000"`, `"400"`, `"N/A"` |
| 5 | SampleType | TEXT | | | 樣本種類 | 處理的樣本類型 | `"FFPE-DNA"`, `"cfDNA"`, `"FFPE-RNA"`, `"virus//RNA"`, `"gDNA"`, `"RNA"`, `"TEST"` |
| 6 | ConsumableLayoutCode | TEXT | | | 耗材擺放代碼(A-B-C-D-E-F) | 六段式耗材擺放配置碼，見下方詳細說明 | `"1-1-1-1-1-1"`, `"0-5-1-1-1-1"` |
| 7 | ExtractionTime | INTEGER | | | 萃取時間 | 整個萃取流程的預估執行秒數，0 表無萃取 | `8100` (=135min), `9300`, `0` |

**索引**:
| 索引名稱 | 欄位 | 唯一 |
|---------|------|:----:|
| IX_FlowMapping_FlowCode | FlowCode | ✓ |

**FlowCode 前綴分類規則**:

| 前綴 | 分類 | 說明 | 範例 |
|-----|------|------|------|
| P | 產品流程 (Product) | 正式出貨產品的標準流程 | P0001, P0002, P0003, P0008 |
| C | 客製流程 (Custom) | 客戶定製的特殊流程 | C0001~C0008 |
| NE | 無萃取 (No Extraction) | 跳過萃取步驟的流程 | NE001~NE003 |
| MT | 維護流程 (Maintenance) | 安裝/校正/維護用 | MT001~MT003 |
| QC | 品管流程 (Quality Control) | 品質控制驗證用 | QC001~QC004 |

**ConsumableLayoutCode 六段代碼詳解**:

格式: `A-B-C-D-E-F`，以 `-` 分隔六個數字。

| 段位 | 代碼 | 說明 | 值域範例 |
|-----|------|------|--------|
| A | 萃取模組配置 | 0=不使用, 1=標準, 2=大容量 | 0, 1, 2 |
| B | 稀釋程式版本 | 0=不使用, 1~7=不同版本 | 0, 1, 3, 4, 5, 6, 7 |
| C | 標準品配置 | 0=不使用, 1=使用 | 0, 1 |
| D | 基本耗材配置 | 0=不使用, 1=使用 | 0, 1 |
| E | PCR 模組配置 | 0=不使用, 1=標準, 2=RNA模式, 3=客製模式 | 0, 1, 2, 3 |
| F | 預留擴展位 | 0=不使用, 1=使用 | 0, 1 |

---

### 2.5 PnidMapping — PNID 編碼映射表

**目前資料量**: 24 筆  
**C# Entity**: `TRIO2026.Core.Entities.PnidMapping`

| # | 欄位名稱 | 資料型別 | NOT NULL | 預設值 | PK | 說明 |
|---|---------|---------|:--------:|--------|:--:|------|
| 0 | Id | INTEGER | ✓ | (自動) | ✓ | 主鍵 |
| 1 | PnidCode | TEXT | ✓ | — | | PNID 編碼（82004, 83015 等） |
| 2 | DescriptionEn | TEXT | | — | | 英文說明 |
| 3 | DescriptionZh | TEXT | | — | | 中文說明 |
| 4 | LinkedProductCode | TEXT | | — | | 關聯的 ProductCode |

**索引**:
| 索引名稱 | 欄位 | 唯一 |
|---------|------|:----:|
| IX_PnidMapping_PnidCode | PnidCode | ✓ |

---

## 三、trio240plus_data.db（檢測數據庫）

### 3.1 TestRecord — 檢測運行記錄表

**目前資料量**: 0 筆（運行時產生）  
**C# Entity**: `TRIO2026.Core.Entities.TestRecord`

| # | 欄位名稱 | 資料型別 | NOT NULL | 預設值 | PK | 說明 |
|---|---------|---------|:--------:|--------|:--:|------|
| 0 | Id | INTEGER | ✓ | (自動) | ✓ | 主鍵 |
| 1 | RunId | TEXT | ✓ | — | | 運行批次 ID（時間戳生成） |
| 2 | FlowName | TEXT | ✓ | — | | 執行的流程名稱 |
| 3 | ProductCode | TEXT | | — | | 產品編碼 |
| 4 | OperatorName | TEXT | | — | | 操作員 |
| 5 | SampleCount | INTEGER | | — | | 樣本數量 |
| 6 | StartTime | TEXT | ✓ | — | | 開始時間 |
| 7 | EndTime | TEXT | | — | | 結束時間 |
| 8 | Status | TEXT | ✓ | 'Running' | | 狀態: Running/Completed/Error/Aborted |
| 9 | ErrorCode | TEXT | | — | | 錯誤碼 |
| 10 | ErrorMessage | TEXT | | — | | 錯誤訊息 |

**索引**:
| 索引名稱 | 欄位 | 唯一 |
|---------|------|:----:|
| IX_TestRecord_RunId | RunId | ✓ |

---

### 3.2 SampleResult — 樣本結果表

**目前資料量**: 0 筆  
**C# Entity**: `TRIO2026.Core.Entities.SampleResult`

| # | 欄位名稱 | 資料型別 | NOT NULL | 預設值 | PK | 說明 |
|---|---------|---------|:--------:|--------|:--:|------|
| 0 | Id | INTEGER | ✓ | (自動) | ✓ | 主鍵 |
| 1 | TestRecordId | INTEGER | ✓ | — | | FK → TestRecord.Id |
| 2 | SampleBarcode | TEXT | | — | | 樣本條碼 |
| 3 | SamplePosition | INTEGER | | — | | 樣本位置編號 |
| 4 | Concentration | REAL | | — | | 濃度結果 |
| 5 | Volume | REAL | | — | | 體積（μL） |
| 6 | QualityFlag | TEXT | | — | | 品質標記: Pass/Fail/Recheck |
| 7 | RawDataJson | TEXT | | — | | 光學原始數據（JSON） |
| 8 | CreatedAt | TEXT | ✓ | — | | 建立時間 |

**外鍵**:
| 來源欄位 | 目標表.欄位 | ON DELETE |
|---------|-----------|----------|
| TestRecordId | TestRecord.Id | CASCADE |

---

### 3.3 ReportSnapshot — 報表快照表

**目前資料量**: 0 筆  
**C# Entity**: `TRIO2026.Core.Entities.ReportSnapshot`

| # | 欄位名稱 | 資料型別 | NOT NULL | 預設值 | PK | 說明 |
|---|---------|---------|:--------:|--------|:--:|------|
| 0 | Id | INTEGER | ✓ | (自動) | ✓ | 主鍵 |
| 1 | TestRecordId | INTEGER | ✓ | — | | FK → TestRecord.Id |
| 2 | ReportType | TEXT | ✓ | — | | 報表類型 |
| 3 | GeneratedAt | TEXT | ✓ | — | | 產生時間 |
| 4 | ContentJson | TEXT | | — | | 報表內容（JSON） |
| 5 | PdfBlob | BLOB | | — | | PDF 二進位 |

**外鍵**:
| 來源欄位 | 目標表.欄位 | ON DELETE |
|---------|-----------|----------|
| TestRecordId | TestRecord.Id | CASCADE |

---

## 四、trio240plus_log.db（日誌記錄庫）

### 4.1 OperationLog — 使用者操作日誌表

**目前資料量**: 0 筆  
**C# Entity**: `TRIO2026.Core.Entities.OperationLog`

| # | 欄位名稱 | 資料型別 | NOT NULL | 預設值 | PK | 說明 |
|---|---------|---------|:--------:|--------|:--:|------|
| 0 | Id | INTEGER | ✓ | (自動) | ✓ | 主鍵 |
| 1 | Timestamp | TEXT | ✓ | — | | 時間戳（ISO8601） |
| 2 | Level | TEXT | ✓ | — | | 日誌等級: Info/Warning/Error |
| 3 | Category | TEXT | ✓ | — | | 分類: UI/Flow/Modbus/System |
| 4 | UserName | TEXT | | — | | 操作者 |
| 5 | Action | TEXT | ✓ | — | | 動作描述 |
| 6 | Detail | TEXT | | — | | 詳細資訊 |

**索引**:
| 索引名稱 | 欄位 | 唯一 |
|---------|------|:----:|
| IX_OperationLog_Timestamp | Timestamp | |
| IX_OperationLog_Category_Level | Category, Level | |

---

### 4.2 CommunicationLog — Modbus 通訊記錄表

**目前資料量**: 0 筆  
**C# Entity**: `TRIO2026.Core.Entities.CommunicationLog`

| # | 欄位名稱 | 資料型別 | NOT NULL | 預設值 | PK | 說明 |
|---|---------|---------|:--------:|--------|:--:|------|
| 0 | Id | INTEGER | ✓ | (自動) | ✓ | 主鍵 |
| 1 | Timestamp | TEXT | ✓ | — | | 時間戳（ISO8601） |
| 2 | Direction | TEXT | ✓ | — | | 方向: Send/Receive |
| 3 | FunctionCode | INTEGER | | — | | Modbus 功能碼（0x03/0x06/0x10） |
| 4 | Address | INTEGER | | — | | 暫存器位址 |
| 5 | DataHex | TEXT | | — | | 原始資料（十六進位） |
| 6 | IsError | INTEGER | ✓ | 0 | | 是否為錯誤回覆 |

**索引**:
| 索引名稱 | 欄位 | 唯一 |
|---------|------|:----:|
| IX_CommunicationLog_Timestamp | Timestamp | |

---

*文件結束*
