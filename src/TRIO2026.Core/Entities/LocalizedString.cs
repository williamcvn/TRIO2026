namespace TRIO2026.Core.Entities;

/// <summary>
/// 多語系字串資源 — 對應資料庫: trio240plus_config.db → LocalizedString 表
/// 
/// 設計目標：
///   1. 所有 UI 文字存放於 DB，方便管理與即時切換語系
///   2. 以 Module + ResourceKey + LanguageCode 三欄位組合為唯一鍵
///   3. 變更 DB 中的語系設定後，UI 可即時重新渲染
///   4. 支援按模組載入，避免一次載入過多資料
///
/// 支援語系：
///   - en      英文（預設）
///   - zh-TW   繁體中文
///   - zh-CN   簡體中文
///   - ja      日語
///
/// 使用範例：
///   Module = "UV",   ResourceKey = "Title",        LanguageCode = "en",    Value = "UV Decontamination"
///   Module = "UV",   ResourceKey = "Title",        LanguageCode = "zh-TW", Value = "UV 消毒"
///   Module = "Common", ResourceKey = "OK",         LanguageCode = "en",    Value = "OK"
///   Module = "Common", ResourceKey = "OK",         LanguageCode = "zh-TW", Value = "確定"
/// </summary>
public class LocalizedString
{
    /// <summary>主鍵（自動遞增）</summary>
    public int Id { get; set; }

    /// <summary>
    /// 功能模組名稱，用於分組載入
    /// 範例: "Common", "UV", "Login", "Menu", "Setting", "Error"
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// 資源鍵值（同模組內唯一識別碼）
    /// 範例: "Title", "Start", "Stop", "DoorErrorMessage"
    /// </summary>
    public string ResourceKey { get; set; } = string.Empty;

    /// <summary>
    /// 語系代碼（IETF BCP 47 格式）
    /// 支援: "en", "zh-TW", "zh-CN", "ja"
    /// </summary>
    public string LanguageCode { get; set; } = "en";

    /// <summary>
    /// 翻譯後的文字內容
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 備註說明（管理用途，程式不使用）
    /// </summary>
    public string? Description { get; set; }
}
