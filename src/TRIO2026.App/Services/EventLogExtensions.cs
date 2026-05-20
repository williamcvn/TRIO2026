using TRIO2026.Core;

namespace TRIO2026.App.Services;

/// <summary>
/// EventLogService 語意化擴充方法 — 簡化 UI 操作追蹤的呼叫
/// 
/// 設計原則：
///   - 每個方法對應一種操作類型，減少呼叫端重複程式碼
///   - 密碼欄位自動遮蔽（isSensitive = true → "***"）
///   - 所有方法都是 null-safe（Instance 為 null 時不拋例外）
/// 
/// 製作者: Office of William
/// </summary>
public static class EventLogExtensions
{
    // ═══════════════════════════════════════
    // 頁面導航
    // ═══════════════════════════════════════

    /// <summary>記錄頁面導航事件</summary>
    /// <param name="fromPage">來源頁面（null = 首次進入）</param>
    /// <param name="toPage">目標頁面</param>
    public static void LogNavigation(this EventLogService? service,
        string? fromPage, string toPage)
    {
        service?.LogInfo("Navigation", "AppShell", ErrorCodes.PageNavigation,
            "頁面導航",
            $"From={fromPage ?? "None"}, To={toPage}");
    }

    // ═══════════════════════════════════════
    // 按鈕點擊
    // ═══════════════════════════════════════

    /// <summary>記錄按鈕點擊事件</summary>
    /// <param name="page">所在頁面名稱</param>
    /// <param name="element">元件名稱或 ID</param>
    /// <param name="detail">額外資訊（可選）</param>
    public static void LogButtonClick(this EventLogService? service,
        string page, string element, string? detail = null)
    {
        service?.LogInfo("UI", page, null,
            "Button Click",
            $"Element={element}, Page={page}" +
            (detail != null ? $", {detail}" : ""));
    }

    // ═══════════════════════════════════════
    // 輸入欄位（失焦時記錄）
    // ═══════════════════════════════════════

    /// <summary>記錄輸入欄位內容（失焦時觸發）</summary>
    /// <param name="page">所在頁面名稱</param>
    /// <param name="field">欄位名稱</param>
    /// <param name="value">輸入值</param>
    /// <param name="isSensitive">是否為敏感欄位（密碼等）→ 自動遮蔽</param>
    public static void LogInput(this EventLogService? service,
        string page, string field, string? value, bool isSensitive = false)
    {
        var safeValue = isSensitive ? "***" : (value ?? "");
        service?.LogInfo("UI", page, null,
            "Input",
            $"Field={field}, Value={safeValue}, Page={page}");
    }

    // ═══════════════════════════════════════
    // UV 操作
    // ═══════════════════════════════════════

    /// <summary>記錄 UV 操作事件</summary>
    /// <param name="action">動作：Start / Stop / Complete / DurationChanged</param>
    /// <param name="detail">額外資訊（如照射時間）</param>
    /// <param name="errorId">對應 ErrorCode（可選）</param>
    public static void LogUvAction(this EventLogService? service,
        string action, string? detail = null, string? errorId = null)
    {
        service?.LogInfo("UV", "UvViewModel", errorId,
            $"UV {action}", detail);
    }

    // ═══════════════════════════════════════
    // 認證操作
    // ═══════════════════════════════════════

    /// <summary>記錄認證事件</summary>
    /// <param name="action">動作：Login / Logout</param>
    /// <param name="username">帳號名稱</param>
    /// <param name="success">是否成功</param>
    /// <param name="detail">額外資訊（失敗原因等）</param>
    public static void LogAuth(this EventLogService? service,
        string action, string? username, bool success, string? detail = null)
    {
        var errorId = action switch
        {
            "Login" when success => ErrorCodes.LoginSuccess,
            "Login" when !success => ErrorCodes.LoginFailed,
            "Logout" => ErrorCodes.UserLogout,
            _ => null
        };

        var level = success ? "Info" : "Warning";

        if (level == "Warning")
            service?.LogWarning("Auth", "LoginViewModel", errorId,
                $"{action} {(success ? "Success" : "Failed")}",
                $"Username={username ?? "unknown"}" + (detail != null ? $", {detail}" : ""));
        else
            service?.LogInfo("Auth", "LoginViewModel", errorId,
                $"{action} {(success ? "Success" : "Failed")}",
                $"Username={username ?? "unknown"}" + (detail != null ? $", {detail}" : ""));
    }

    // ═══════════════════════════════════════
    // 選單操作
    // ═══════════════════════════════════════

    /// <summary>記錄選單操作</summary>
    public static void LogMenuAction(this EventLogService? service,
        string action, string? detail = null)
    {
        service?.LogInfo("UI", "UserMenu", null,
            $"Menu {action}", detail);
    }
}
