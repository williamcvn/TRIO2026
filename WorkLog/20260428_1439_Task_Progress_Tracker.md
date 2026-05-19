# TRIO2026 任務進度追蹤

> **文件編號**: TRIO2026-WL-003  
> **撰寫**: Office of William  
> **日期**: 2026-04-28 14:39  
> **任務範圍**: 資料庫建立全階段任務清單與完成狀態  

---

## Phase 0: 環境與專案建立
- [x] 安裝 .NET 10 SDK (10.0.203 LTS)
- [x] 建立 `global.json` 鎖定 SDK 版本
- [x] 建立 Solution + 專案結構（Core / Data / DbInitializer）
- [x] 安裝 NuGet 套件 (EF Core Sqlite 10.0.7 + Design 10.0.7)
- [x] 建立元件紀錄文件 `Dependencies_and_Components.md`
- [x] 複製 `implementation_plan.md` 至 `DocumentsAndInformation/`

## Phase 1: Entity 類別（12 個）
- [x] SystemConfig.cs — 通用 KV 配置
- [x] CommandDefinition.cs — 58 個指令定義
- [x] UserAccount.cs — BCrypt 密碼雜湊
- [x] FlowDefinition.cs — 流程定義
- [x] FlowStep.cs — 流程步驟
- [x] FlowMapping.cs — 產品↔流程映射
- [x] PnidMapping.cs — PNID 編碼映射
- [x] TestRecord.cs — 檢測運行記錄
- [x] SampleResult.cs — 樣本結果
- [x] ReportSnapshot.cs — 報表快照
- [x] OperationLog.cs — 操作日誌
- [x] CommunicationLog.cs — Modbus 通訊日誌

## Phase 2: DbContext（4 個）+ Initializer
- [x] ConfigDbContext.cs (trio240plus_config.db)
- [x] MainDbContext.cs (trio240plus_main.db)
- [x] DataDbContext.cs (trio240plus_data.db)
- [x] LogDbContext.cs (trio240plus_log.db)
- [x] DatabaseInitializer.cs (PRAGMA + EnsureCreated + Seed)

## Phase 3: .db 檔案生成 + 驗證
- [x] `dotnet build` — 0 錯誤 0 警告
- [x] 執行 DbInitializer 生成 4 個 .db
- [x] 驗證 12 張表結構正確
- [x] 驗證索引/外鍵/UNIQUE
- [x] 驗證 WAL 模式

## Phase 4: Seed Data 植入
- [x] CommandDefinition — 29 筆（X_Flow_t 列舉）
- [x] SystemConfig — 2,497 筆（12 個 .ini 解析）
- [x] FlowMapping — 18 筆（flowinfo.ini）
- [x] PnidMapping — 24 筆（flowinfo.ini）
- [x] UserAccount — 3 筆（admin/service/operator）
- [x] FlowDefinition — 10 筆（.flow 檔案）
- [x] FlowStep — 2,091 筆（.flow 步驟）
- [x] 驗證所有資料筆數 — 總計 4,672 筆

## 後續待辦
- [ ] 正式部署時替換 UserAccount 的 PLACEHOLDER 密碼
- [ ] 安裝 BCrypt.Net-Next 套件產生真正的密碼雜湊
- [ ] 遷移舊系統的歷史 TestRecord / SampleResult 資料
- [ ] 實作 WPF UI 層的資料庫存取介面

---

*文件結束*
