namespace TRIO2026.Core.Entities;

/// <summary>
/// 事件代碼定義對照表 — 預定義 + 動態註冊的事件代碼
/// 
/// 代碼前綴：
///   INF-XNNN = Info（資訊事件）
///   WRN-XNNN = Warning（警告事件）
///   ERR-XNNN = Error / Fatal（錯誤/致命）
/// 
/// 雙模運作：
///   1. 預定義：開發時在 EventCodeDefinitionSeed 登記已知事件
///   2. 動態註冊：運行時遇到未預定義的錯誤，系統自動新增到此表
/// 
/// 多語系支援：
///   UserMessageKey 對應 LocalizedString 的 Key，
///   顯示在 UI 時透過 LocalizationService 取得當前語言的訊息。
///   若無對應 Key，使用 UserMessageFallback 作為預設顯示。
/// 
/// 製作者: Office of William
/// </summary>
public class EventCodeDefinition
{
    public int Id { get; set; }

    /// <summary>事件代碼（唯一），如 INF-1004, WRN-2001, ERR-1001</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>分類: System / UV / Auth / Hardware / Navigation / Config</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>嚴重程度: Info / Warning / Error / Fatal</summary>
    public string Severity { get; set; } = "Error";

    /// <summary>簡短標題（技術人員用，英文）</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>詳細說明（開發/維護團隊閱讀）</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>建議處置方式（技術人員參考）</summary>
    public string? Resolution { get; set; }

    /// <summary>
    /// 多語系使用者訊息 Key — 對應 LocalizedString 表
    /// 格式: Error.{Code}
    /// 若找不到則使用 UserMessageFallback
    /// </summary>
    public string? UserMessageKey { get; set; }

    /// <summary>
    /// 使用者友善訊息（預設語言，當多語系 Key 找不到時使用）
    /// </summary>
    public string? UserMessageFallback { get; set; }
}
