# TRIO2026 重構專案 — 元件與依賴清單

> **文件編號**: TRIO2026-DEP-001  
> **版本**: 1.0  
> **撰寫**: Office of William  
> **日期**: 2026-04-28  
> **目的**: 記錄專案所使用的所有外部元件、NuGet 套件及工具的來源與版本

---

## 一、.NET SDK

| 項目 | 版本 | 類型 | 來源 | 費用 |
|------|------|------|------|------|
| .NET 10 SDK | 10.0.203 | LTS（支援至 2028-11） | winget install Microsoft.DotNet.SDK.10 | $0 |
| .NET 10 Runtime | 10.0.7 | 隨 SDK | 同上 | $0 |

- **下載頁面**: https://dotnet.microsoft.com/en-us/download/dotnet/10.0
- **安裝方式**: `winget install Microsoft.DotNet.SDK.10 --accept-source-agreements --accept-package-agreements`
- **全域鎖定**: `global.json` 設定 `sdk.version: 10.0.203`

---

## 二、NuGet 套件

### 2.1 TRIO2026.Data 專案

| 套件名稱 | 版本 | 用途 | 授權 | NuGet 頁面 |
|----------|------|------|------|-----------|
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.7 | SQLite 資料庫 Provider | Apache-2.0 | https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite |
| Microsoft.EntityFrameworkCore.Design | 10.0.7 | EF Core 設計時期工具（Migration 生成） | Apache-2.0 | https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Design |

### 2.2 未來預計新增（尚未安裝）

| 套件名稱 | 預計版本 | 用途 | 授權 |
|----------|---------|------|------|
| BCrypt.Net-Next | 4.x | 密碼雜湊（UserAccount） | MIT |
| Microsoft.EntityFrameworkCore.Tools | 10.x | dotnet ef CLI 工具 | Apache-2.0 |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.x | 未來 PostgreSQL 遷移 | PostgreSQL License |

---

## 三、外部工具

| 工具 | 版本 | 用途 | 來源 | 備份位置 |
|------|------|------|------|---------|
| .NET 10 SDK Installer | 10.0.203 | 開發環境 SDK | winget（自動下載） | 已安裝至 `C:\Program Files\dotnet\` |

> [!NOTE]
> 目前所有依賴均透過 NuGet 安裝，無需額外下載。  
> 若日後有非 NuGet 的元件需求，將先下載至 `D:\TRIO2026\Tools\` 備份後再安裝。

---

## 四、專案參考關係

```
TRIO2026.Core (classlib, net10.0)
  └── 無外部依賴（純 Entity 模型）

TRIO2026.Data (classlib, net10.0)
  ├── → TRIO2026.Core
  ├── NuGet: Microsoft.EntityFrameworkCore.Sqlite 10.0.7
  └── NuGet: Microsoft.EntityFrameworkCore.Design 10.0.7

DbInitializer (console, net10.0)
  ├── → TRIO2026.Core
  └── → TRIO2026.Data
```

---

*文件結束*
