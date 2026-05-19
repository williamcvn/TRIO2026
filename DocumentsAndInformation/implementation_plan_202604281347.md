# TRIO2026 SQLite 資料庫建立 — 實作計畫

## 目標

在 `D:\TRIO2026` 專案目錄中，根據 `SQLite_Database_Design_Specification.md` 規格書的定義，建立四個 SQLite 資料庫檔案及完整的 .NET 專案結構（Entity + DbContext + Migration + 初始化邏輯）。

## 環境確認

| 項目 | 狀態 |
|------|------|
| .NET SDK | ✅ 9.0.302 |
| 目標路徑 | ✅ `D:\TRIO2026`（空目錄，可存取） |
| 資料庫規格 | ✅ 已讀取完整 13 張表定義 |

## 提議的專案結構

根據規格書 §6 定義的三層架構，建立以下專案結構：

```
D:\TRIO2026\
├── TRIO2026.sln                         ← Solution 檔
├── src/
│   ├── TRIO2026.Core/                   ← 純模型層（無 DB 依賴）
│   │   ├── Entities/
│   │   │   ├── SystemConfig.cs          ← 通用配置 KV
│   │   │   ├── CommandDefinition.cs     ← 58 個指令定義
│   │   │   ├── UserAccount.cs           ← 使用者帳號
│   │   │   ├── FlowDefinition.cs        ← 流程定義
│   │   │   ├── FlowStep.cs              ← 流程步驟
│   │   │   ├── FlowMapping.cs           ← 產品 ↔ 流程映射
│   │   │   ├── PnidMapping.cs           ← PNID 映射
│   │   │   ├── TestRecord.cs            ← 檢測記錄
│   │   │   ├── SampleResult.cs          ← 樣本結果
│   │   │   ├── ReportSnapshot.cs        ← 報表快照
│   │   │   ├── OperationLog.cs          ← 操作日誌
│   │   │   └── CommunicationLog.cs      ← Modbus 通訊日誌
│   │   └── Interfaces/
│   │       ├── IConfigRepository.cs
│   │       ├── IFlowRepository.cs
│   │       ├── IDataRepository.cs
│   │       └── ILogRepository.cs
│   │
│   └── TRIO2026.Data/                   ← 資料存取層
│       ├── Contexts/
│       │   ├── ConfigDbContext.cs        ← trio240plus_config.db
│       │   ├── MainDbContext.cs          ← trio240plus_main.db
│       │   ├── DataDbContext.cs          ← trio240plus_data.db
│       │   └── LogDbContext.cs           ← trio240plus_log.db
│       ├── Extensions/
│       │   └── DatabaseInitializer.cs   ← 初始化 + PRAGMA 設定
│       └── TRIO2026.Data.csproj
│
├── Database/                            ← 生成的 .db 檔案位置
│   ├── trio240plus_config.db
│   ├── trio240plus_main.db
│   ├── trio240plus_data.db
│   └── trio240plus_log.db
│
└── tools/
    └── DbInitializer/                  ← Console App，執行初始化 + 建表
        └── Program.cs
```

## Proposed Changes

### 1. Solution 與專案建立

使用 `dotnet` CLI 建立：
- `dotnet new sln -n TRIO2026`
- `dotnet new classlib -n TRIO2026.Core`
- `dotnet new classlib -n TRIO2026.Data`
- `dotnet new console -n DbInitializer`（工具用來初始化 DB）
- 加入必要 NuGet：`Microsoft.EntityFrameworkCore.Sqlite`、`Microsoft.EntityFrameworkCore.Design`

### 2. TRIO2026.Core — 13 個 Entity 類別

根據規格書 §3，建立以下 Entity：

**trio240plus_config.db (2 表)**
- `SystemConfig` — Category + Key + Value KV 配置
- `CommandDefinition` — 58 個指令，5 組參數（Type/Label/Options）

**trio240plus_main.db (5 表)**
- `UserAccount` — BCrypt 密碼雜湊
- `FlowDefinition` — 流程定義
- `FlowStep` — 流程步驟（FK → FlowDefinition）
- `FlowMapping` — ProductCode ↔ FlowName
- `PnidMapping` — PNID 編碼映射

**trio240plus_data.db (3 表)**
- `TestRecord` — 運行記錄（RunId）
- `SampleResult` — 樣本結果（FK → TestRecord）
- `ReportSnapshot` — 報表快照 + PDF Blob

**trio240plus_log.db (2 表)**
- `OperationLog` — 操作日誌
- `CommunicationLog` — Modbus 通訊記錄

### 3. TRIO2026.Data — 4 個 DbContext

每個 DbContext 對應一個 .db 檔案，在 `OnModelCreating` 中設定：
- Unique 約束
- 索引
- 外鍵（CASCADE）
- 預設值

### 4. DatabaseInitializer

封裝 `EnsureCreated()` + PRAGMA 設定邏輯：
- WAL mode
- synchronous = NORMAL
- foreign_keys = ON
- cache_size = -2000
- busy_timeout = 5000

### 5. DbInitializer 工具

Console App 執行一次即可建立四個空的 .db 檔案（含表結構）。

## Open Questions

> [!IMPORTANT]
> **SDK 版本選擇**：偵測到 .NET 9.0 SDK，規格書中寫的是 .NET 8。是否要使用 .NET 9？
> - 使用 .NET 9：最新功能，但 EF Core 套件版本需改為 9.x
> - 使用 .NET 8：與規格書一致（長期支援版 LTS）
> 
> **建議使用 .NET 8 (LTS)**，因醫療設備應優先選擇長期支援版本。

> [!NOTE]
> **本次範圍僅建立空表結構**：不包含 Seed Data（DB-06~DB-11）、Repository（DB-13~DB-21）及遷移工具（DB-22~DB-26）。這些將在後續階段實作。

## Verification Plan

### Automated Tests
1. `dotnet build` — 確認全部專案編譯通過
2. 執行 DbInitializer — 確認 4 個 .db 檔案生成
3. 用 SQLite 指令驗證每個 .db 的表結構正確性（`sqlite3 <db> ".tables"` 及 `.schema`）
