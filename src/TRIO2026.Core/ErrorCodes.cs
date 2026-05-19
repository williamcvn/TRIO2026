namespace TRIO2026.Core;

/// <summary>
/// 事件代碼常數 — 對應 EventCodeDefinition 表的 Code 欄位
/// 
/// 命名規則：
///   INF-XNNN = Info（資訊事件）
///   WRN-XNNN = Warning（警告事件）
///   ERR-XNNN = Error / Fatal（錯誤/致命）
///   X = 分類碼（1=System, 2=Auth, 3=UV, 4=Hardware, 5=Config, 6=Navigation）
/// 
/// 程式碼中統一使用此常數引用，確保與 DB 對照表一致。
/// 新增時需同步更新此類別與 EventCodeDefinitionSeed。
/// 
/// 製作者: Office of William
/// </summary>
public static class ErrorCodes
{
    // ── 1xxx System ──
    public const string UnhandledException = "ERR-1001";
    public const string DatabaseConnectionFailure = "ERR-1002";
    public const string EventLogWriteFailure = "WRN-1003";
    public const string AppStartup = "INF-1004";
    public const string AppShutdown = "INF-1005";

    // ── 2xxx Auth ──
    public const string LoginFailed = "WRN-2001";
    public const string LoginSuccess = "INF-2002";
    public const string UserLogout = "INF-2003";

    // ── 3xxx UV ──
    public const string UvStart = "INF-3001";
    public const string UvStop = "WRN-3002";
    public const string UvComplete = "INF-3003";
    public const string UvDoorInterrupted = "ERR-3004";
    public const string UvLampFailure = "ERR-3005";
    public const string UvConfigLoadFailure = "ERR-3006";

    // ── 4xxx Hardware ──
    public const string HardwareCommunicationFailure = "ERR-4001";

    // ── 5xxx Config ──
    public const string ConfigLoadFailure = "WRN-5001";

    // ── 6xxx Navigation ──
    public const string PageNavigation = "INF-6001";
}
