using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TRIO2026.App.Services;

/// <summary>
/// Token 加密服務 — 使用 DPAPI 加密「記住密碼」Token
/// 
/// 加密方式: Windows DPAPI (DataProtectionScope.CurrentUser)
/// 儲存位置: %LocalAppData%/TRIO2026/remembered_token.dat
/// 安全性: Token 綁定到 Windows 使用者帳號，其他帳號或機器無法解密
/// </summary>
public class TokenService
{
    private static readonly string TokenDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TRIO2026");

    private static readonly string TokenPath = Path.Combine(TokenDir, "remembered_token.dat");

    /// <summary>
    /// 儲存記住密碼的 Token（DPAPI 加密）
    /// </summary>
    public void SaveRememberedCredentials(string username, string password)
    {
        try
        {
            Directory.CreateDirectory(TokenDir);
            var plainText = $"{username}\n{password}";
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encrypted = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(TokenPath, encrypted);
        }
        catch
        {
            // 加密或寫入失敗靜默處理
        }
    }

    /// <summary>
    /// 讀取記住密碼的 Token（DPAPI 解密）
    /// </summary>
    /// <returns>成功時回傳 (username, password)，失敗回傳 null</returns>
    public (string Username, string Password)? LoadRememberedCredentials()
    {
        try
        {
            if (!File.Exists(TokenPath))
                return null;

            var encrypted = File.ReadAllBytes(TokenPath);
            var plainBytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            var plainText = Encoding.UTF8.GetString(plainBytes);
            var parts = plainText.Split('\n', 2);
            if (parts.Length == 2)
                return (parts[0], parts[1]);
        }
        catch
        {
            // 解密失敗（可能是不同使用者或檔案損壞），刪除舊 Token
            ClearRememberedCredentials();
        }
        return null;
    }

    /// <summary>
    /// 清除記住密碼的 Token
    /// </summary>
    public void ClearRememberedCredentials()
    {
        try
        {
            if (File.Exists(TokenPath))
                File.Delete(TokenPath);
        }
        catch { }
    }
}
