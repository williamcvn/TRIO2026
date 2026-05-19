using System.Security.Cryptography;
using System.Text.Json;

namespace TRIO2026.Data.Seeding;

/// <summary>
/// 種子密碼供應器 — 從外部 JSON 檔讀取或自動產生安全密碼
/// 
/// ⚠️ 資安設計：
///   1. 密碼從 Database/seed_credentials.json 讀取（不編譯進 DLL）
///   2. 若檔案不存在 → 自動產生隨機強密碼 + 建立檔案
///   3. DbInitializer 完成後可呼叫 DeleteCredentialFile() 銷毀檔案
///   4. 密碼產生結果輸出至 Console（僅首次部署可見）
/// 
/// seed_credentials.json 格式：
///   {
///     "admin": "YourPassword",
///     "service": "YourPassword",
///     "operator": "YourPassword"
///   }
/// 
/// 製作者: Office of William
/// </summary>
public static class SeedCredentialProvider
{
    private const string CredentialFileName = "seed_credentials.json";

    /// <summary>
    /// 讀取或產生種子密碼
    /// </summary>
    /// <param name="databaseDir">Database 目錄路徑</param>
    /// <returns>帳號→密碼對應</returns>
    public static Dictionary<string, string> LoadOrGenerate(string databaseDir)
    {
        var filePath = Path.Combine(databaseDir, CredentialFileName);

        if (File.Exists(filePath))
        {
            Console.WriteLine($"[SeedCredential] 從外部檔案讀取密碼: {CredentialFileName}");
            var json = File.ReadAllText(filePath);
            var creds = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return creds ?? GenerateAndSave(filePath);
        }

        Console.WriteLine($"[SeedCredential] 密碼檔不存在，自動產生安全密碼");
        return GenerateAndSave(filePath);
    }

    /// <summary>
    /// 銷毀密碼檔（DbInitializer 完成後呼叫）
    /// </summary>
    public static void DeleteCredentialFile(string databaseDir)
    {
        var filePath = Path.Combine(databaseDir, CredentialFileName);
        if (File.Exists(filePath))
        {
            // 安全刪除：先覆寫再刪除
            var bytes = new byte[new FileInfo(filePath).Length];
            RandomNumberGenerator.Fill(bytes);
            File.WriteAllBytes(filePath, bytes);
            File.Delete(filePath);
            Console.WriteLine($"[SeedCredential] ⚠️ 密碼檔已安全銷毀: {CredentialFileName}");
        }
    }

    /// <summary>產生隨機強密碼並寫入檔案</summary>
    private static Dictionary<string, string> GenerateAndSave(string filePath)
    {
        var creds = new Dictionary<string, string>
        {
            ["admin"] = GenerateSecurePassword(16),
            ["service"] = GenerateSecurePassword(16),
            ["operator"] = GenerateSecurePassword(12),
        };

        // 寫入檔案供管理員確認
        var json = JsonSerializer.Serialize(creds, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(filePath, json);

        // Console 輸出（僅首次部署可見）
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║     ⚠️ 初始密碼（請立即記錄並變更）         ║");
        Console.WriteLine("╠══════════════════════════════════════════════╣");
        foreach (var (user, pwd) in creds)
            Console.WriteLine($"║  {user,-12} : {pwd,-28} ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");

        return creds;
    }

    /// <summary>產生密碼學安全的隨機密碼</summary>
    private static string GenerateSecurePassword(int length)
    {
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%&*";
        const string all = upper + lower + digits + special;

        var password = new char[length];
        var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);

        // 確保至少包含各類字元
        password[0] = upper[bytes[0] % upper.Length];
        password[1] = lower[bytes[1] % lower.Length];
        password[2] = digits[bytes[2] % digits.Length];
        password[3] = special[bytes[3] % special.Length];

        for (int i = 4; i < length; i++)
            password[i] = all[bytes[i] % all.Length];

        // Fisher-Yates 洗牌
        for (int i = length - 1; i > 0; i--)
        {
            rng.GetBytes(bytes, 0, 1);
            int j = bytes[0] % (i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }
}
