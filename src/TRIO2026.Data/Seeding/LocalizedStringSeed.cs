using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Seeding;

/// <summary>
/// 多語系字串種子資料
/// 
/// 語系代碼：
///   - en      英文（預設）
///   - zh-TW   繁體中文
///   - zh-CN   簡體中文
///   - ja      日語
///
/// Module 分類：
///   - Common   共用字串（OK、Cancel 等跨功能使用）
///   - UV       UV Decontamination 功能專用
///   - Login    登入功能（預留）
///   - Menu     主選單功能（預留）
///   - Error    通用錯誤訊息（預留）
///
/// 新增語系步驟：
///   1. 在此檔案新增對應 LanguageCode 的資料列
///   2. 重新執行 DbInitializer 或手動 INSERT
///   3. 在 SystemConfig 中設定 system.language 為新語系代碼
///
/// 製作者: Office of William
/// </summary>
public static class LocalizedStringSeed
{
    private static readonly string[] Languages = { "en", "zh-TW", "zh-CN", "ja" };

    public static List<LocalizedString> GetSeedData()
    {
        var seeds = new List<LocalizedString>();
        int id = 1;

        // ══════════════════════════════════════════
        // Common 模組 — 跨功能共用字串
        // ══════════════════════════════════════════
        seeds.AddRange(CreateGroup(ref id, "Common", "OK",
            "OK", "確定", "确定", "OK"));
        seeds.AddRange(CreateGroup(ref id, "Common", "Cancel",
            "Cancel", "取消", "取消", "キャンセル"));
        seeds.AddRange(CreateGroup(ref id, "Common", "Confirm",
            "Confirm", "確認", "确认", "確認"));
        seeds.AddRange(CreateGroup(ref id, "Common", "Back",
            "Back", "返回", "返回", "戻る"));
        seeds.AddRange(CreateGroup(ref id, "Common", "Close",
            "Close", "關閉", "关闭", "閉じる"));
        seeds.AddRange(CreateGroup(ref id, "Common", "Ready",
            "● Ready", "● 就緒", "● 就绪", "● 準備完了"));
        seeds.AddRange(CreateGroup(ref id, "Common", "Yes",
            "Yes", "是", "是", "はい"));
        seeds.AddRange(CreateGroup(ref id, "Common", "No",
            "No", "否", "否", "いいえ"));

        // ══════════════════════════════════════════
        // UV 模組 — UV Decontamination 功能
        // ══════════════════════════════════════════

        // 頁面
        seeds.AddRange(CreateGroup(ref id, "UV", "Title",
            "UV Decontamination", "UV 消毒", "UV 消毒", "UV 除染"));
        seeds.AddRange(CreateGroup(ref id, "UV", "HomeTooltip",
            "Back to HOME", "返回主畫面", "返回主界面", "ホームに戻る"));

        // 按鈕
        seeds.AddRange(CreateGroup(ref id, "UV", "Start",
            "Start", "開始", "开始", "スタート"));
        seeds.AddRange(CreateGroup(ref id, "UV", "Stop",
            "Stop", "停止", "停止", "ストップ"));

        // 完成提示
        seeds.AddRange(CreateGroup(ref id, "UV", "CompleteTitle",
            "UV Decontamination", "UV 消毒", "UV 消毒", "UV 除染"));
        seeds.AddRange(CreateGroup(ref id, "UV", "CompleteMessage",
            "UV light is completed. Please back to HOME screen.",
            "UV 照射完成。請返回主畫面。",
            "UV 照射完成。请返回主界面。",
            "UV照射が完了しました。ホーム画面に戻ってください。"));

        // 停止確認
        seeds.AddRange(CreateGroup(ref id, "UV", "StopConfirmTitle",
            "Information", "提示", "提示", "情報"));
        seeds.AddRange(CreateGroup(ref id, "UV", "StopConfirmMessage",
            "Are you certain about stopping the UV light process?",
            "確定要停止 UV 照射嗎？",
            "确定要停止 UV 照射吗？",
            "UV照射を停止してもよろしいですか？"));

        // 門板警示
        seeds.AddRange(CreateGroup(ref id, "UV", "DoorErrorTitle",
            "Error!", "錯誤！", "错误！", "エラー！"));
        seeds.AddRange(CreateGroup(ref id, "UV", "DoorErrorMessage",
            "The door is open. Please close the door to proceed.",
            "門板已開啟，請關閉門板以繼續。",
            "门板已打开，请关闭门板以继续。",
            "ドアが開いています。続行するにはドアを閉じてください。"));
        seeds.AddRange(CreateGroup(ref id, "UV", "DoorNotClosed",
            "The door is not closed.", "門板尚未關閉", "门板尚未关闭", "ドアが閉まっていません"));

        // ══════════════════════════════════════════
        // UserMenu 模組 — 共用使用者選單
        // ══════════════════════════════════════════
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "Home",
            "Home", "返回主畫面", "返回主界面", "ホーム"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "SwitchLanguage",
            "Switch Language", "切換語系", "切换语言", "言語切替"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "Logout",
            "Logout", "登出", "登出", "ログアウト"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "CloseApp",
            "Close Application", "關閉應用程式", "关闭应用程序", "アプリを閉じる"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "LogoutConfirmTitle",
            "Logout Confirmation", "登出確認", "登出确认", "ログアウト確認"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "LogoutConfirmMessage",
            "Do you want to logout?", "是否要登出系統？", "是否要登出系统？", "ログアウトしますか？"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "LogoutAndClose",
            "Logout & Close", "登出並關閉系統", "登出并关闭系统", "ログアウトして閉じる"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "CloseConfirmTitle",
            "Close Application", "關閉視窗", "关闭窗口", "アプリを閉じる"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "CloseConfirmMessage",
            "Do you want to close the application?",
            "是否要關閉應用程式？",
            "是否要关闭应用程序？",
            "アプリケーションを閉じますか？"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "InsufficientPermission",
            "Insufficient Permission", "權限不足", "权限不足", "権限不足"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "InsufficientPermissionMessage",
            "Your account does not have permission to close the application.\nService level or above is required.",
            "您的帳號權限不足，無法關閉應用程式。\n需要 Service 等級以上的權限。",
            "您的账号权限不足，无法关闭应用程序。\n需要 Service 等级以上的权限。",
            "アカウントの権限が不足しています。\nService レベル以上の権限が必要です。"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "AuthTitle",
            "Identity Verification", "身分驗證", "身份验证", "本人確認"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "AuthMessage",
            "Please enter your credentials to confirm close permission.",
            "請輸入帳號密碼以確認關閉權限。",
            "请输入账号密码以确认关闭权限。",
            "閉じる権限を確認するため、認証情報を入力してください。"));

        // ══════════════════════════════════════════
        // Login 模組 — 登入驗證錯誤訊息
        // ══════════════════════════════════════════
        seeds.AddRange(CreateGroup(ref id, "Login", "UserNotFound",
            "Account not found", "帳號不存在", "账号不存在", "アカウントが見つかりません"));
        seeds.AddRange(CreateGroup(ref id, "Login", "WrongPassword",
            "Wrong password", "密碼錯誤", "密码错误", "パスワードが間違っています"));
        seeds.AddRange(CreateGroup(ref id, "Login", "AccountDisabled",
            "Account disabled", "帳號已停用", "账号已停用", "アカウントが無効です"));
        seeds.AddRange(CreateGroup(ref id, "Login", "AccountLocked",
            "Account locked, please try again later",
            "帳號已鎖定，請稍後再試",
            "账号已锁定，请稍后再试",
            "アカウントがロックされています。しばらくしてから再試行してください"));
        seeds.AddRange(CreateGroup(ref id, "Login", "AuthFailed",
            "Authentication failed", "驗證失敗", "验证失败", "認証に失敗しました"));

        // ══════════════════════════════════════════
        // Error 模組 — 錯誤提示視窗用
        // ══════════════════════════════════════════
        seeds.AddRange(CreateGroup(ref id, "Error", "Title",
            "TRIO2026 Error", "TRIO2026 錯誤", "TRIO2026 错误", "TRIO2026 エラー"));
        seeds.AddRange(CreateGroup(ref id, "Error", "ReportHint",
            "Please report this Error ID to technical support.",
            "請將此 Error ID 提供給技術支援人員。",
            "请将此 Error ID 提供给技术支持人员。",
            "このエラーIDを技術サポートに報告してください。"));

        // 各 ErrorId 對應使用者訊息
        seeds.AddRange(CreateGroup(ref id, "Error", "ERR-1001",
            "An unexpected error occurred. Please restart the application.",
            "發生未預期的錯誤，請重新啟動應用程式。",
            "发生未预期的错误，请重新启动应用程序。",
            "予期しないエラーが発生しました。アプリケーションを再起動してください。"));
        seeds.AddRange(CreateGroup(ref id, "Error", "ERR-1002",
            "Database connection failed. Please restart the application.",
            "資料庫連線失敗，請重新啟動應用程式。",
            "数据库连接失败，请重新启动应用程序。",
            "データベース接続に失敗しました。アプリケーションを再起動してください。"));
        seeds.AddRange(CreateGroup(ref id, "Error", "ERR-3004",
            "Door opened during UV operation. Close the door to resume.",
            "UV 運行期間門板被開啟，請關閉門板以恢復。",
            "UV 运行期间门板被打开，请关闭门板以恢复。",
            "UV動作中にドアが開きました。ドアを閉じて再開してください。"));
        seeds.AddRange(CreateGroup(ref id, "Error", "ERR-3005",
            "UV lamp failed to start. Please contact maintenance.",
            "UV 燈管啟動失敗，請聯繫維護團隊。",
            "UV 灯管启动失败，请联系维护团队。",
            "UVランプの起動に失敗しました。メンテナンスに連絡してください。"));
        seeds.AddRange(CreateGroup(ref id, "Error", "ERR-9000",
            "An unknown error occurred. Please report the Error ID.",
            "發生未知錯誤，請回報 Error ID。",
            "发生未知错误，请回报 Error ID。",
            "不明なエラーが発生しました。エラーIDを報告してください。"));

        // ══════════════════════════════════════════
        // Menu 模組 — 主選單頁面
        // ══════════════════════════════════════════
        seeds.AddRange(CreateGroup(ref id, "Menu", "IntelliPlex",
            "IntelliPlex", "IntelliPlex", "IntelliPlex", "IntelliPlex"));
        seeds.AddRange(CreateGroup(ref id, "Menu", "IntelliPlexSub",
            "Program", "程式設定", "程序设置", "プログラム"));
        seeds.AddRange(CreateGroup(ref id, "Menu", "Custom",
            "Custom", "自訂", "自定义", "カスタム"));
        seeds.AddRange(CreateGroup(ref id, "Menu", "CustomSub",
            "Program", "程式設定", "程序设置", "プログラム"));
        seeds.AddRange(CreateGroup(ref id, "Menu", "Data",
            "Data", "數據", "数据", "データ"));
        seeds.AddRange(CreateGroup(ref id, "Menu", "DataSub",
            "History & Results", "歷史紀錄與結果", "历史记录与结果", "履歴と結果"));
        seeds.AddRange(CreateGroup(ref id, "Menu", "Setting",
            "Setting", "設定", "设置", "設定"));
        seeds.AddRange(CreateGroup(ref id, "Menu", "SettingSub",
            "System Config", "系統設定", "系统设置", "システム設定"));
        seeds.AddRange(CreateGroup(ref id, "Menu", "UV",
            "UV", "UV", "UV", "UV"));
        seeds.AddRange(CreateGroup(ref id, "Menu", "UVSub",
            "Decontamination", "消毒", "消毒", "除染"));
        seeds.AddRange(CreateGroup(ref id, "Menu", "StatusReady",
            "● Ready", "● 就緒", "● 就绪", "● 準備完了"));

        // ══════════════════════════════════════════
        // UserMenu 模組 — Service Mode 切換
        // ══════════════════════════════════════════
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "ServiceMode",
            "Service Mode", "Service Mode", "Service Mode", "サービスモード"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "ServiceModeTitle",
            "Service Mode Login", "Service Mode 登入", "Service Mode 登录", "サービスモード ログイン"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "ServiceModeMessage",
            "Enter Service credentials to proceed.",
            "請輸入 Service 帳號密碼以進入。",
            "请输入 Service 账号密码以进入。",
            "サービスアカウントの認証情報を入力してください。"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "ServiceModeInsufficientRole",
            "Service role or higher is required.",
            "需要 Service 以上角色權限。",
            "需要 Service 以上角色权限。",
            "Service 以上の権限が必要です。"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "ExitServiceModeTitle",
            "Exit Service Mode", "退出 Service Mode", "退出 Service Mode", "サービスモード終了"));
        seeds.AddRange(CreateGroup(ref id, "UserMenu", "ExitServiceModeMessage",
            "Do you want to exit Service Mode and return to normal operation?",
            "是否要退出 Service Mode 並返回一般操作模式？",
            "是否要退出 Service Mode 并返回一般操作模式？",
            "サービスモードを終了して通常操作に戻りますか？"));

        // ══════════════════════════════════════════
        // ServiceMode 模組 — Service Mode 頁面
        // ══════════════════════════════════════════
        seeds.AddRange(CreateGroup(ref id, "ServiceMode", "Title",
            "Service Mode", "Service Mode", "Service Mode", "サービスモード"));
        seeds.AddRange(CreateGroup(ref id, "ServiceMode", "Placeholder",
            "This page is under development.", "此頁面功能開發中。", "此页面功能开发中。", "このページは開発中です。"));

        return seeds;
    }

    /// <summary>
    /// 建立同一 ResourceKey 的四語系資料群組
    /// </summary>
    /// <param name="id">自動遞增 ID 計數器</param>
    /// <param name="module">功能模組</param>
    /// <param name="key">資源鍵值</param>
    /// <param name="en">英文</param>
    /// <param name="zhTw">繁體中文</param>
    /// <param name="zhCn">簡體中文</param>
    /// <param name="ja">日語</param>
    private static List<LocalizedString> CreateGroup(
        ref int id, string module, string key,
        string en, string zhTw, string zhCn, string ja)
    {
        var values = new[] { en, zhTw, zhCn, ja };
        var result = new List<LocalizedString>();

        for (int i = 0; i < Languages.Length; i++)
        {
            result.Add(new LocalizedString
            {
                Id = id++,
                Module = module,
                ResourceKey = key,
                LanguageCode = Languages[i],
                Value = values[i],
                Description = $"{module}.{key}"
            });
        }

        return result;
    }
}
