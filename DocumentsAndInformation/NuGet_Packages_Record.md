# TRIO2026 NuGet 套件紀錄

> **撰寫**: Office of William  
> **日期**: 2026-04-29  
> **版本**: 1.1（新增 WPF 登入頁所需套件）

---

## 套件清單

| # | 套件名稱 | 版本 | 所屬專案 | 授權 | 用途 |
|---|---------|------|---------|------|------|
| 1 | Microsoft.EntityFrameworkCore.Sqlite | 10.0.7 | TRIO2026.Data | MIT | SQLite EF Core Provider |
| 2 | Microsoft.EntityFrameworkCore.Design | 10.0.7 | TRIO2026.Data | MIT | EF Core 設計工具（Migration 等） |
| 3 | **BCrypt.Net-Next** | **4.0.3** | TRIO2026.App, DbInitializer | MIT | 密碼 BCrypt 雜湊與驗證 |
| 4 | **Microsoft.Extensions.DependencyInjection** | **10.0.7** | TRIO2026.App | MIT | WPF 依賴注入容器 |

## 注意事項

- **BCrypt.Net-Next**: 用於取代舊系統明碼密碼儲存，workFactor=12
- **DI 版本**: 必須與 EF Core 10.0.7 的間接依賴一致（≥10.0.7），否則會觸發 NU1605 降級錯誤

---

*文件結束*
