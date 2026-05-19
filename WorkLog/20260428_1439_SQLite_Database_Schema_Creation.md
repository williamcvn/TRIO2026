# TRIO2026 SQLite 資料庫架構建立 — 工作紀錄

> **文件編號**: TRIO2026-WL-001  
> **撰寫**: Office of William  
> **日期**: 2026-04-28 14:39  
> **任務範圍**: 資料庫 Schema 建立 + .db 檔案生成 + PRAGMA 設定  

---

## 一、任務目標

將 TRIO240 舊系統的散落儲存方式（.ini、.csv、.flow）遷移至統一的 SQLite 四庫架構，以 .NET 10 + EF Core 10 為核心技術堆疊。

---

## 二、技術環境

| 項目 | 版本 |
|------|------|
| .NET SDK | 10.0.203 (LTS, 支援至 2028-11) |
| EF Core Sqlite | 10.0.7 |
| EF Core Design | 10.0.7 |
| SQLite | 內建於 EF Core |
| OS | Windows |

---

## 三、專案結構

```
D:\TRIO2026\
├── TRIO2026.sln
├── global.json                          ← .NET 10 SDK 鎖定
├── src/
│   ├── TRIO2026.Core/                   ← 12 個 Entity (net10.0)
│   │   └── Entities/
│   │       ├── SystemConfig.cs
│   │       ├── CommandDefinition.cs
│   │       ├── UserAccount.cs
│   │       ├── FlowDefinition.cs
│   │       ├── FlowStep.cs
│   │       ├── FlowMapping.cs
│   │       ├── PnidMapping.cs
│   │       ├── TestRecord.cs
│   │       ├── SampleResult.cs
│   │       ├── ReportSnapshot.cs
│   │       ├── OperationLog.cs
│   │       └── CommunicationLog.cs
│   └── TRIO2026.Data/                   ← 4 DbContext + Seeding (net10.0)
│       ├── Contexts/
│       │   ├── ConfigDbContext.cs
│       │   ├── MainDbContext.cs
│       │   ├── DataDbContext.cs
│       │   └── LogDbContext.cs
│       ├── Extensions/
│       │   └── DatabaseInitializer.cs
│       └── Seeding/
│           ├── CommandDefinitionSeed.cs
│           ├── FlowInfoSeed.cs
│           └── UserAccountSeed.cs
├── tools/
│   ├── DbInitializer/Program.cs         ← Console App 建表工具
│   ├── import_system_config.py          ← 12 INI → SystemConfig
│   ├── import_flow_definitions.py       ← .flow → FlowDefinition+Step
│   └── verify_schema.py                ← 驗證腳本
├── Database/                            ← 四個 .db 檔案
│   ├── trio240plus_config.db
│   ├── trio240plus_main.db
│   ├── trio240plus_data.db
│   └── trio240plus_log.db
└── DocumentsAndInformation/
    ├── implementation_plan.md
    └── Dependencies_and_Components.md
```

---

## 四、資料庫 Schema 設計

### 4.1 trio240plus_config.db（機器配置庫）

| 表名 | 用途 | 主要欄位 | 索引/約束 |
|------|------|---------|----------|
| SystemConfig | 通用 KV 配置 | Category, Key, Value, DataType | UNIQUE(Category, Key) |
| CommandDefinition | 指令定義 | Id(0~28), Name, Arg0~4(Type/Label/Options) | PK: Id (手動指定) |

### 4.2 trio240plus_main.db（業務核心庫）

| 表名 | 用途 | 主要欄位 | 索引/約束 |
|------|------|---------|----------|
| UserAccount | 使用者帳號 | Username, PasswordHash, RoleLevel | UNIQUE(Username) |
| FlowDefinition | 流程定義 | FlowName, TotalSteps | UNIQUE(FlowName) |
| FlowStep | 流程步驟 | FlowDefinitionId, StepOrder, CommandId, Arg0~4 | FK→FlowDefinition(CASCADE) |
| FlowMapping | 產品↔流程映射 | ProductCode, FlowName | UNIQUE(ProductCode) |
| PnidMapping | PNID 編碼映射 | PnidCode, DescriptionEn/Zh | UNIQUE(PnidCode) |

### 4.3 trio240plus_data.db（檢測數據庫）

| 表名 | 用途 | 主要欄位 | 索引/約束 |
|------|------|---------|----------|
| TestRecord | 檢測運行記錄 | RunId, FlowName, Status | UNIQUE(RunId) |
| SampleResult | 樣本結果 | TestRecordId, Concentration | FK→TestRecord(CASCADE) |
| ReportSnapshot | 報表快照 | TestRecordId, ReportType, PdfBlob | FK→TestRecord(CASCADE) |

### 4.4 trio240plus_log.db（日誌記錄庫）

| 表名 | 用途 | 主要欄位 | 索引/約束 |
|------|------|---------|----------|
| OperationLog | 使用者操作日誌 | Timestamp, Level, Category, Action | IDX(Timestamp), IDX(Category,Level) |
| CommunicationLog | Modbus 通訊記錄 | Timestamp, Direction, FunctionCode | IDX(Timestamp) |

---

## 五、PRAGMA 設定

所有四個資料庫均套用以下 PRAGMA：

```sql
PRAGMA journal_mode = WAL;        -- 允許讀寫並發
PRAGMA synchronous = NORMAL;      -- 平衡效能與安全
PRAGMA foreign_keys = ON;         -- 啟用外鍵約束
PRAGMA cache_size = -2000;        -- 2MB 快取
PRAGMA busy_timeout = 5000;       -- 忙碌等待 5 秒
```

---

## 六、建置與驗證結果

| 驗證項目 | 結果 |
|---------|------|
| `dotnet build` | ✅ 0 錯誤 0 警告 |
| DbInitializer 執行 | ✅ 4 個 .db 成功建立 |
| 表結構驗證 | ✅ 12 張表全部正確 |
| 索引/外鍵/UNIQUE 驗證 | ✅ 全部生效 |
| WAL 模式驗證 | ✅ 四個 DB 均為 WAL |

---

*文件結束*
