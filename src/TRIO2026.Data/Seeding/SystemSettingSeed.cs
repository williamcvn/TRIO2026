using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Seeding;

/// <summary>
/// SystemSetting 種子資料（system_config.db）
/// 
/// 設定分類：
///   - UserMenu   使用者選單相關設定
///   - System     系統全域設定
///   - EventLog   事件日誌歸檔設定
///   - LoginUI    登入介面設定
///   - Auth       認證設定
///   - AppClose   關閉控制設定
///   - Device     裝置運作模式設定
/// 
/// 製作者: Office of William
/// </summary>
public static class SystemSettingSeed
{
    public static List<SystemSetting> GetSeedData()
    {
        return new List<SystemSetting>
        {
            // ── UserMenu 設定 ──
            new()
            {
                Id = 1,
                Category = "UserMenu",
                Key = "auto_close_seconds",
                Value = "10",
                Description = "使用者選單自動關閉秒數（預設 10 秒，0=不自動關閉）",
                Remark = "✅ 已實作 — UserMenuControl.cs 讀取控制自動關閉"
            },

            // ── System 設定 ──
            new()
            {
                Id = 2,
                Category = "System",
                Key = "multilanguage_enabled",
                Value = "1",
                Description = "是否啟用多語系功能（1=啟用, 0=停用，停用時以 English 為預設語言）",
                Remark = "✅ 已實作 — UserMenuControl.cs 控制語系按鈕可見性"
            },

            // ── EventLog 設定 ──
            new()
            {
                Id = 3,
                Category = "EventLog",
                Key = "archive_interval",
                Value = "monthly",
                Description = "事件日誌歸檔區間（monthly=按月, weekly=按週, quarterly=按季）",
                Remark = "✅ 已實作 — EventLogArchiveService.cs 讀取控制歸檔週期"
            },
            new()
            {
                Id = 4,
                Category = "EventLog",
                Key = "backup_schedule_days",
                Value = "30",
                Description = "歸檔 DB 搬移至備份目錄的排程天數（預設 30 天）",
                Remark = "✅ 已實作 — EventLogArchiveService.cs 控制備份搬移排程"
            },
            new()
            {
                Id = 5,
                Category = "EventLog",
                Key = "last_archive_date",
                Value = "",
                Description = "上次歸檔執行日期（系統自動更新，格式 yyyy-MM-dd）",
                Remark = "✅ 已實作 — EventLogArchiveService.cs 自動寫入"
            },
            new()
            {
                Id = 6,
                Category = "EventLog",
                Key = "last_backup_date",
                Value = "",
                Description = "上次備份搬移執行日期（系統自動更新，格式 yyyy-MM-dd）",
                Remark = "✅ 已實作 — EventLogArchiveService.cs 自動寫入"
            },

            // ── Login UI 設定（轉移自 trio240plus_config.db Id 2498-2502） ──
            new()
            {
                Id = 7,
                Category = "LoginUI",
                Key = "show_user_dropdown",
                Value = "0",
                Description = "登入頁面是否顯示使用者下拉清單（0=停用, 1=啟用）",
                Remark = "✅ 已實作 — LoginPage 根據此設定切換帳號文字框/下拉選單"
            },
            new()
            {
                Id = 8,
                Category = "LoginUI",
                Key = "remember_password_enabled",
                Value = "1",
                Description = "是否允許記住密碼功能（0=停用, 1=啟用）",
                Remark = "⬜ 未實作 — 記住密碼功能存在但未讀取此開關"
            },
            new()
            {
                Id = 9,
                Category = "LoginUI",
                Key = "max_failed_attempts",
                Value = "5",
                Description = "最大連續登入失敗次數（超過後鎖定帳號）",
                Remark = "⚠️ 部分實作 — AuthService 使用硬編碼 const=5，未讀取 DB"
            },
            new()
            {
                Id = 10,
                Category = "LoginUI",
                Key = "lockout_minutes",
                Value = "15",
                Description = "帳號鎖定持續時間（分鐘）",
                Remark = "⚠️ 部分實作 — AuthService 使用硬編碼 const=15，未讀取 DB"
            },
            new()
            {
                Id = 11,
                Category = "LoginUI",
                Key = "session_timeout_minutes",
                Value = "30",
                Description = "Session 閒置逾時（分鐘，0=不逾時）",
                Remark = "⬜ 未實作 — 無閒置偵測計時器"
            },

            // ── Auth 設定（轉移自 trio240plus_config.db Id 2503-2505） ──
            new()
            {
                Id = 12,
                Category = "Auth",
                Key = "login_required",
                Value = "0",
                Description = "是否啟動帳號密碼檢查（0=免登入, 1=需登入）",
                Remark = "✅ 已實作 — AppShell.cs 讀取決定起始頁面"
            },
            new()
            {
                Id = 13,
                Category = "Auth",
                Key = "init_wait_seconds",
                Value = "2",
                Description = "Init 畫面等待秒數",
                Remark = "✅ 已實作 — InitPage.cs 讀取控制倒數秒數"
            },
            new()
            {
                Id = 14,
                Category = "Auth",
                Key = "default_role_level",
                Value = "1",
                Description = "免登入時預設角色等級（1=Operator, 2=Service, 3=Admin）",
                Remark = "✅ 已實作 — AppShell.cs 免登入模式設定 Guest Session"
            },

            // ── Guest Account 設定 ──
            new()
            {
                Id = 18,
                Category = "Auth",
                Key = "guest_account_username",
                Value = "local_operator",
                Description = "免登入模式專用帳號的 username（對應 main.db User 表）",
                Remark = "✅ 已實作 — AppShell.cs 免登入模式從 DB 載入此帳號"
            },
            new()
            {
                Id = 19,
                Category = "Auth",
                Key = "guest_account_display_name",
                Value = "Local Operator",
                Description = "免登入模式右上角顯示的名稱",
                Remark = "✅ 已實作 — SessionService + UserMenuControl 讀取顯示"
            },

            // ── App Close 設定（轉移自 trio240plus_config.db Id 2506-2508） ──
            new()
            {
                Id = 15,
                Category = "AppClose",
                Key = "button_enabled",
                Value = "0",
                Description = "關閉按鈕是否顯示（0=隱藏, 1=顯示）",
                Remark = "✅ 已實作 — LoginPage.cs 控制 CloseButton 可見性"
            },
            new()
            {
                Id = 16,
                Category = "AppClose",
                Key = "esc_key_enabled",
                Value = "1",
                Description = "ESC 鍵關閉是否啟用（0=停用, 1=啟用）",
                Remark = "✅ 已實作 — AppShell.cs PreviewKeyDown 攔截"
            },
            new()
            {
                Id = 17,
                Category = "AppClose",
                Key = "alt_f4_enabled",
                Value = "0",
                Description = "Alt+F4 關閉是否啟用（0=停用, 1=啟用）",
                Remark = "✅ 已實作 — AppShell.cs Closing 事件攔截"
            },

            // ── Device 設定 ──
            new()
            {
                Id = 20,
                Category = "Device",
                Key = "operation_mode",
                Value = "IntelliPlex",
                Description = "裝置運作模式（Combo=雙模式皆啟用, IntelliPlex=僅 IntelliPlex, Custom=僅 Custom）",
                Remark = "✅ 已實作 — MenuPage.cs 讀取控制功能按鈕啟用狀態"
            },

            // ── System 設定（續） ──
            new()
            {
                Id = 21,
                Category = "System",
                Key = "default_language",
                Value = "en",
                Description = "系統預設語系（當未登入或免登入模式時使用，例: en, zh-TW）",
                Remark = "✅ 已實作 — App.xaml.cs 啟動時初始化 + UserMenuControl 免登入模式切換語系時寫入"
            },
        };
    }
}
