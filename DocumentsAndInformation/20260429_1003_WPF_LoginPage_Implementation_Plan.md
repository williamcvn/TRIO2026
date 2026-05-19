# TRIO2026 WPF 登入頁面實作計畫

## 目標

建立 WPF 登入頁面，支援觸控螢幕與鍵盤/滑鼠操作，包含「記住密碼」功能（使用加密 Token）。
此為帳號密碼權限控管的 Phase 1 + Phase 2 實作。

---

## 前置條件

- 目前專案無 .sln 檔案，需建立
- 目前無 WPF 專案，需建立 `TRIO2026.App`（WPF 應用程式）
- 現有專案 `TRIO2026.Core`（net10.0）和 `TRIO2026.Data`（net10.0）需改為 `net10.0-windows` 以支援 WPF 依賴鏈

---

## User Review Required

> [!IMPORTANT]
> **TRIO2026.Core 和 TRIO2026.Data 的 TFM 需要從 `net10.0` 改為 `net10.0-windows`**，因為 WPF 只能在 Windows 上運行。如果未來有跨平台需求（如 Linux 部署），需要另外設計分離架構。目前 TRIO240 為 Windows 觸控設備，因此此變更合理。

> [!IMPORTANT]
> **登入頁面解析度**: 舊系統使用 `1920×1200` 觸控螢幕。新系統登入頁面是否沿用此解析度？

---

## Open Questions

> [!IMPORTANT]
> 1. **啟動行為**: 系統啟動時是否直接進入登入頁面？還是像舊系統一樣在 Service Mode 時才觸發登入？
> 2. **帳號下拉選單**: 舊系統登入時有使用者下拉選單，新系統是否保留此設計？還是改為手動輸入帳號？
> 3. **使用者頭像**: 舊系統支援使用者照片（imagedata BLOB），新系統是否需要？

---

## Proposed Changes

### 1. Solution 與 WPF 專案建立

#### [NEW] TRIO2026.sln
- 建立 Solution 檔案，包含所有專案

#### [NEW] src/TRIO2026.App/TRIO2026.App.csproj
- WPF 應用程式專案（`net10.0-windows`，`<UseWPF>true</UseWPF>`）
- 參考 `TRIO2026.Data`
- NuGet: `BCrypt.Net-Next`、`Microsoft.Extensions.DependencyInjection`

#### [MODIFY] [TRIO2026.Core.csproj](file:///D:/TRIO2026/src/TRIO2026.Core/TRIO2026.Core.csproj)
- TFM: `net10.0` → `net10.0-windows`（WPF 依賴鏈要求）

#### [MODIFY] [TRIO2026.Data.csproj](file:///D:/TRIO2026/src/TRIO2026.Data/TRIO2026.Data.csproj)
- TFM: `net10.0` → `net10.0-windows`

---

### 2. Core 層擴展

#### [MODIFY] [UserAccount.cs](file:///D:/TRIO2026/src/TRIO2026.Core/Entities/UserAccount.cs)
新增欄位：
- `FailedLoginCount` (int) — 連續登入失敗次數
- `LockedUntil` (string?) — 鎖定到期時間
- `PasswordChangedAt` (string?) — 密碼變更時間
- `DisplayName` (string?) — 顯示名稱

#### [NEW] src/TRIO2026.Core/Enums/AuthResult.cs
```csharp
public enum AuthResult
{
    Success, UserNotFound, WrongPassword,
    AccountDisabled, AccountLocked
}
```

#### [NEW] src/TRIO2026.Core/Enums/RoleLevel.cs
```csharp
public enum RoleLevel { Operator = 1, Service = 2, Admin = 3 }
```

#### [NEW] src/TRIO2026.Core/Interfaces/IAuthService.cs
- `AuthResult Login(string username, string password)`
- `void Logout()`
- `bool ChangePassword(string oldPwd, string newPwd)`

#### [NEW] src/TRIO2026.Core/Interfaces/ISessionService.cs
- `UserAccount? CurrentUser { get; }`
- `bool IsAuthenticated { get; }`
- `bool HasPermission(RoleLevel required)`

---

### 3. Data 層擴展

#### [MODIFY] [MainDbContext.cs](file:///D:/TRIO2026/src/TRIO2026.Data/Contexts/MainDbContext.cs)
- UserAccount 新增 `FailedLoginCount`、`LockedUntil` 等欄位配置

---

### 4. WPF App 專案（TRIO2026.App）

#### [NEW] src/TRIO2026.App/App.xaml + App.xaml.cs
- DI 容器配置（ServiceCollection）
- DbContext 註冊
- 啟動時顯示 LoginWindow

#### [NEW] src/TRIO2026.App/Services/AuthService.cs
- BCrypt 密碼驗證
- 登入失敗計數 + 鎖定邏輯
- 操作日誌寫入

#### [NEW] src/TRIO2026.App/Services/SessionService.cs
- 當前使用者管理
- 權限檢查

#### [NEW] src/TRIO2026.App/Services/TokenService.cs
- 「記住密碼」功能
- 使用 DPAPI（`System.Security.Cryptography.ProtectedData`）加密
- Token 存入 `%LocalAppData%/TRIO2026/remembered_token.dat`
- 加密方式: `DataProtectionScope.CurrentUser`（綁定 Windows 帳號）

#### [NEW] src/TRIO2026.App/Views/LoginWindow.xaml
- 全螢幕登入視窗（1920×1200 或自適應）
- 觸控友善設計：
  - 大型按鈕（最小 48dp 觸控區域）
  - 大字體輸入框
  - 帳號下拉選單（ComboBox）
  - 密碼輸入框（PasswordBox）
  - 「記住密碼」核取方塊
  - 登入按鈕
  - 內建虛擬觸控鍵盤（可選，Windows 自帶 TabTip）
- 視覺設計：
  - 深色背景 + 毛玻璃卡片
  - TRIO 品牌 Logo
  - 漸變色登入按鈕
  - 平滑動畫（淡入、抖動提示錯誤）

#### [NEW] src/TRIO2026.App/ViewModels/LoginViewModel.cs
- MVVM 模式
- `ICommand LoginCommand`
- `ICommand ToggleRememberCommand`
- 資料繫結：Username、Password、RememberMe、ErrorMessage、IsLoading

#### [NEW] src/TRIO2026.App/Helpers/RelayCommand.cs
- ICommand 實作（MVVM 輔助）

#### [NEW] src/TRIO2026.App/Converters/BoolToVisibilityConverter.cs
- 布林轉可見性轉換器

---

### 5. Seed Data 更新

#### [MODIFY] [UserAccountSeed.cs](file:///D:/TRIO2026/src/TRIO2026.Data/Seeding/UserAccountSeed.cs)
- 密碼改為使用 BCrypt 真正雜湊（移除 PLACEHOLDER）
- 新增 FailedLoginCount、LockedUntil 等欄位初始值

---

## Verification Plan

### Automated Tests
```bash
# 建置所有專案
dotnet build

# 啟動 WPF 應用程式
dotnet run --project src/TRIO2026.App
```

### Manual Verification
1. 啟動應用程式 → 登入頁面正確顯示
2. 輸入正確帳密 → 登入成功（顯示主畫面佔位）
3. 輸入錯誤密碼 → 顯示錯誤提示 + 抖動動畫
4. 連續 5 次錯誤 → 帳號鎖定提示
5. 勾選「記住密碼」→ 下次啟動自動填入
6. 觸控操作測試（若有觸控設備）
