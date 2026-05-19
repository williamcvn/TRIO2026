# TRIO2026 資料庫 Schema 管理策略：導入 EF Core Migrations

> 製作者：Office of William  
> 建立日期：2026-04-30  
> 版本：1.0

---

## 1. 問題回顧：2026-04-30 資料遺失事件

### 事件經過

1. 新增 `RoleDefinition` 資料表（FK 關聯 `UserAccount.RoleLevel`）
2. 舊 `trio240plus_main.db` 沒有此表 → `EnsureCreated()` 無法對已存在的 DB 新增表
3. 為解決問題，刪除所有 .db 檔案重建 → **SystemConfig 等手動建立的測試資料遺失**
4. 從 `E:\temp\Database` 還原備份 → main.db 因新舊 schema 欄位差異需逐表遷移

### 根因分析

| 根因 | 說明 |
|------|------|
| **缺少 Migration 機制** | 使用 `EnsureCreated()` 只能建立新 DB，無法更新現有 DB 的 schema |
| **手動刪除 DB** | 沒有備份流程，直接刪除造成不可逆的資料遺失 |
| **Schema 與 DB 不同步** | Entity 類別變更後，沒有對應的版本化 schema 變更記錄 |

### 關鍵教訓

> [!CAUTION]
> `EnsureCreated()` **不適合**有 schema 演進需求的專案。
> 它只在 DB 完全不存在時建立，對已存在的 DB 不做任何變更。

---

## 2. 解決方案：EF Core Migrations

### 2.1 什麼是 Migrations？

EF Core Migrations 是**版本化的 schema 變更系統**：

```
Entity 類別變更 → 產生 Migration 檔案 → 套用到現有 DB
                    (差異比較)          (ALTER TABLE, ADD COLUMN...)
```

每次 schema 變更都是一個 Migration，包含：
- `Up()` — 套用變更（新增表、加欄位）
- `Down()` — 回滾變更（刪除表、移除欄位）

### 2.2 與 EnsureCreated 的差異

| 特性 | EnsureCreated | Migrations |
|------|:---:|:---:|
| 新建 DB | ✅ | ✅ |
| 更新現有 DB schema | ❌ | ✅ |
| 版本化記錄 | ❌ | ✅ |
| 回滾能力 | ❌ | ✅ |
| 資料保全 | ❌ | ✅ |
| CI/CD 整合 | ❌ | ✅ |

### 2.3 工作流程

```
開發者修改 Entity 類別
       ↓
dotnet ef migrations add <名稱>     ← 產生 Migration 檔案
       ↓
Code Review / 確認
       ↓
dotnet ef database update            ← 套用到 DB（ALTER TABLE...）
       ↓
DB schema 已更新，資料完整保留 ✅
```

---

## 3. 實作架構

### 3.1 專案結構

```
TRIO2026.Data/
├── Contexts/
│   ├── MainDbContext.cs          ← 業務核心（UserAccount, Flow...）
│   ├── ConfigDbContext.cs        ← 機器配置（SystemConfig, Command...）
│   └── LogDbContext.cs           ← 操作日誌
├── Migrations/
│   ├── Main/                     ← MainDbContext 的 Migrations
│   │   └── 20260430_InitialCreate.cs
│   ├── Config/                   ← ConfigDbContext 的 Migrations
│   │   └── 20260430_InitialCreate.cs
│   └── Log/                      ← LogDbContext 的 Migrations
│       └── 20260430_InitialCreate.cs
├── DesignTimeDbContextFactory.cs ← CLI 工具用的 Factory
└── Seeding/
    └── ...
```

### 3.2 Design-Time Factory

EF Core CLI 需要一個 Factory 來建立 DbContext（因為 App 的 DI 容器在 CLI 環境不可用）：

```csharp
public class MainDbContextFactory : IDesignTimeDbContextFactory<MainDbContext>
{
    public MainDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MainDbContext>()
            .UseSqlite("Data Source=D:\\TRIO2026\\Database\\trio240plus_main.db")
            .Options;
        return new MainDbContext(options);
    }
}
```

---

## 4. 常用指令

```bash
# 切到 Data 專案目錄
cd D:\TRIO2026\src\TRIO2026.Data

# === 新增 Migration ===
dotnet ef migrations add <名稱> --context MainDbContext --output-dir Migrations/Main
dotnet ef migrations add <名稱> --context ConfigDbContext --output-dir Migrations/Config

# === 套用到 DB ===
dotnet ef database update --context MainDbContext
dotnet ef database update --context ConfigDbContext

# === 回滾到指定版本 ===
dotnet ef database update <Migration名稱> --context MainDbContext

# === 產生 SQL 腳本（不直接執行） ===
dotnet ef migrations script --context MainDbContext -o migration.sql
```

---

## 5. 注意事項

### ⚠️ 操作前必須備份

```bash
# 備份指令（建議加入 Migration 腳本中）
copy D:\TRIO2026\Database\trio240plus_main.db D:\TRIO2026\Database\db_backups\main_YYYYMMDD.db
```

### ⚠️ 初次導入

已存在的 DB 需要先建立「基準 Migration」（記錄當前 schema），再標記為已套用：

```bash
dotnet ef migrations add InitialCreate --context MainDbContext --output-dir Migrations/Main
dotnet ef database update --context MainDbContext
```

### ⚠️ 種子資料

Migrations 支援在 `Up()` 中植入種子資料：

```csharp
migrationBuilder.InsertData(
    table: "RoleDefinition",
    columns: new[] { "Level", "Code", "DisplayName", ... },
    values: new object[] { 1, "Operator", "操作員", ... }
);
```
