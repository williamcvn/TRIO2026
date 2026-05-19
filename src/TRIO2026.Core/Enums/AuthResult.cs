namespace TRIO2026.Core.Enums;

/// <summary>
/// 認證結果列舉
/// </summary>
public enum AuthResult
{
    /// <summary>登入成功</summary>
    Success,
    /// <summary>使用者不存在</summary>
    UserNotFound,
    /// <summary>密碼錯誤</summary>
    WrongPassword,
    /// <summary>帳號已停用</summary>
    AccountDisabled,
    /// <summary>帳號已鎖定（失敗次數過多）</summary>
    AccountLocked
}
