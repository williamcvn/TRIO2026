using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Seeding;

/// <summary>
/// 預設使用者帳號種子資料（trio240plus_main.db — 舊版驗證用）
/// 
/// ⚠️ 資安設計：
///   密碼不寫在程式碼中，改從外部設定檔讀取。
///   與 UserSeed 共用 seed_credentials.json。
/// 
/// 製作者: Office of William
/// </summary>
public static class UserAccountSeed
{
    /// <summary>
    /// 取得種子資料。密碼由外部提供。
    /// </summary>
    public static List<UserAccount> GetSeedData(
        Dictionary<string, string> credentials,
        Func<string, string>? passwordHasher = null)
    {
        var now = DateTime.UtcNow.ToString("o");
        var hash = passwordHasher ?? (pwd => $"$2a$12$PLACEHOLDER_{pwd}");

        return new List<UserAccount>
        {
            new()
            {
                Id = 1,
                Username = "admin",
                PasswordHash = hash(credentials.GetValueOrDefault("admin", "")),
                RoleLevel = 3,
                IsActive = 1,
                CreatedAt = now,
                DisplayName = "Administrator",
            },
            new()
            {
                Id = 2,
                Username = "service",
                PasswordHash = hash(credentials.GetValueOrDefault("service", "")),
                RoleLevel = 2,
                IsActive = 1,
                CreatedAt = now,
                DisplayName = "Service Engineer",
            },
            new()
            {
                Id = 3,
                Username = "operator",
                PasswordHash = hash(credentials.GetValueOrDefault("operator", "")),
                RoleLevel = 1,
                IsActive = 1,
                CreatedAt = now,
                DisplayName = "Operator",
            },
        };
    }
}
