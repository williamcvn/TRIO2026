using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Seeding;

/// <summary>
/// 正式 User 表種子資料（main.db）
/// 
/// ⚠️ 資安設計：
///   密碼不寫在程式碼中，改從外部設定檔 Database/seed_credentials.json 讀取。
///   DbInitializer 執行後會自動刪除該檔案，避免密碼殘留。
///   若檔案不存在，則以隨機密碼初始化並輸出至 Console（僅限首次部署）。
/// 
///   seed_credentials.json 格式：
///   {
///     "admin": "YourAdminPassword",
///     "service": "YourServicePassword",
///     "operator": "YourOperatorPassword"
///   }
/// 
/// 製作者: Office of William
/// </summary>
public static class UserSeed
{
    /// <summary>
    /// 取得種子資料。密碼由外部提供（不寫在程式碼中）。
    /// </summary>
    /// <param name="credentials">帳號→密碼的對應（從外部檔案讀取）</param>
    /// <param name="passwordHasher">BCrypt 雜湊函數</param>
    public static List<User> GetSeedData(
        Dictionary<string, string> credentials,
        Func<string, string>? passwordHasher = null)
    {
        var now = DateTime.UtcNow.ToString("o");
        var hash = passwordHasher ?? (pwd => $"$2a$12$PLACEHOLDER_{pwd}");

        return new List<User>
        {
            new()
            {
                Id = 1,
                Username = "admin",
                PasswordHash = hash(credentials.GetValueOrDefault("admin", "")),
                RoleLevel = 3,
                IsActive = 1,
                CreatedAt = now,
                CreatedBy = "SYSTEM",
                DisplayName = "Administrator",
                ForcePasswordChange = 1, // 強制首次登入變更密碼
            },
            new()
            {
                Id = 2,
                Username = "service",
                PasswordHash = hash(credentials.GetValueOrDefault("service", "")),
                RoleLevel = 2,
                IsActive = 1,
                CreatedAt = now,
                CreatedBy = "SYSTEM",
                DisplayName = "Service Engineer",
                ForcePasswordChange = 1,
            },
            new()
            {
                Id = 3,
                Username = "operator",
                PasswordHash = hash(credentials.GetValueOrDefault("operator", "")),
                RoleLevel = 1,
                IsActive = 1,
                CreatedAt = now,
                CreatedBy = "SYSTEM",
                DisplayName = "Operator",
                ForcePasswordChange = 1,
            },

            // ── 免登入模式專用帳號（不需密碼） ──
            new()
            {
                Id = 100,
                Username = "local_operator",
                PasswordHash = "",              // 免登入，不需密碼
                RoleLevel = 1,                  // Operator（固定）
                IsActive = 1,
                CreatedAt = now,
                CreatedBy = "SYSTEM",
                DisplayName = "Local Operator",
                ForcePasswordChange = 0,
                Notes = "免登入模式專用帳號。固定 Operator 權限。",
            },

            // ── Guest 特殊帳號（免密碼登入，由 SystemSetting 控制啟停） ──
            new()
            {
                Id = 99,
                Username = "guest",
                PasswordHash = "",              // 免密碼
                RoleLevel = 1,                  // Operator（固定）
                IsActive = 1,
                CreatedAt = now,
                CreatedBy = "SYSTEM",
                DisplayName = "Anonymous",
                ForcePasswordChange = 0,
                Notes = "Guest 特殊帳號。免密碼登入，由 SystemSetting guest_login_enabled 控制啟停。",
            },
        };
    }
}
