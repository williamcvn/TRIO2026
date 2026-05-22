using Microsoft.EntityFrameworkCore;
using TRIO2026.Data.Contexts;
using TRIO2026.Data.Seeding;

namespace TRIO2026.Data.Extensions;

/// <summary>
/// 資料庫初始化器：建立資料庫檔案、套用表結構、設定 PRAGMA、植入種子資料。
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>Database 目錄的根路徑</summary>
    private static string _databaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");

    /// <summary>
    /// 設定資料庫目錄的根路徑（預設為執行目錄下的 Database 子目錄）
    /// </summary>
    public static void SetDatabaseDirectory(string path)
    {
        _databaseDir = path;
    }

    /// <summary>
    /// 取得指定資料庫檔案的完整路徑
    /// </summary>
    public static string GetDatabasePath(string dbFileName)
    {
        return Path.Combine(_databaseDir, dbFileName);
    }

    /// <summary>
    /// 初始化全部四個資料庫：建立目錄、建表、設定 PRAGMA、植入種子資料
    /// </summary>
    /// <summary>
    /// 密碼雜湊函數（由外部注入，如 BCrypt.HashPassword）
    /// </summary>
    public static Func<string, string>? PasswordHasher { get; set; }

    public static async Task InitializeAllAsync()
    {
        // 確保 Database 目錄存在
        Directory.CreateDirectory(_databaseDir);

        Console.WriteLine($"[DatabaseInitializer] 資料庫目錄: {_databaseDir}");

        // 載入或產生種子密碼（從外部檔案，不編譯進 DLL）
        var credentials = SeedCredentialProvider.LoadOrGenerate(_databaseDir);

        // 初始化資料庫
        await InitializeSystemConfigDbAsync();
        await InitializeAppMainDbAsync(credentials);
        await InitializeEventLogDbAsync();

        // 初始化完成後安全銷毀密碼檔
        SeedCredentialProvider.DeleteCredentialFile(_databaseDir);

        Console.WriteLine("[DatabaseInitializer] 全部初始化完成");
    }

    // [已移除] InitializeConfigDbAsync — trio240plus_config.db 已廢棄

    /// <summary>初始化 SystemConfig DB（system_config.db）+ Seed Data</summary>
    private static async Task InitializeSystemConfigDbAsync()
    {
        const string dbFile = "system_config.db";
        var dbPath = GetDatabasePath(dbFile);
        Console.WriteLine($"  -> 初始化系統配置庫 ({dbFile})...");

        var options = new DbContextOptionsBuilder<SystemConfigDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        await using var context = new SystemConfigDbContext(options);
        await context.Database.MigrateAsync();
        Console.WriteLine($"    資料庫 Migration 完成");

        await SetPragmasAsync(context);

        // 植入 UvTimerOption 種子資料
        if (!await context.UvTimerOptions.AnyAsync())
        {
            var uvSeeds = UvTimerOptionSeed.GetSeedData();
            context.UvTimerOptions.AddRange(uvSeeds);
            await context.SaveChangesAsync();
            Console.WriteLine($"    已植入 {uvSeeds.Count} 筆 UV 照射時間選項");
        }
        else
        {
            Console.WriteLine($"    UV 照射時間選項已存在，跳過植入");
        }

        // 植入 LocalizedString 多語系種子資料（增量：只補入缺少的 key）
        {
            var i18nSeeds = LocalizedStringSeed.GetSeedData();
            var existingKeys = await context.LocalizedStrings
                .Select(s => s.Module + "." + s.ResourceKey + "." + s.LanguageCode)
                .ToListAsync();
            var existingSet = new HashSet<string>(existingKeys);

            var newSeeds = i18nSeeds
                .Where(s => !existingSet.Contains(s.Module + "." + s.ResourceKey + "." + s.LanguageCode))
                .ToList();

            if (newSeeds.Count > 0)
            {
                // 重新分配 ID，避免主鍵衝突
                var maxId = await context.LocalizedStrings.AnyAsync()
                    ? await context.LocalizedStrings.MaxAsync(s => s.Id)
                    : 0;
                foreach (var seed in newSeeds)
                {
                    seed.Id = ++maxId;
                }

                context.LocalizedStrings.AddRange(newSeeds);
                await context.SaveChangesAsync();
                Console.WriteLine($"    已補入 {newSeeds.Count} 筆多語系字串");
            }
            else
            {
                Console.WriteLine($"    多語系字串已是最新，無需補入");
            }
        }

        // 確保 SystemSetting 表有 Remark 欄位（schema 升級）
        {
            var conn = context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
            using var pragmaCmd = conn.CreateCommand();
            pragmaCmd.CommandText = "PRAGMA table_info(SystemSetting)";
            var hasRemark = false;
            using (var reader = await pragmaCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    if (reader.GetString(1) == "Remark") hasRemark = true;
                }
            }
            if (!hasRemark)
            {
                using var alterCmd = conn.CreateCommand();
                alterCmd.CommandText = "ALTER TABLE SystemSetting ADD COLUMN Remark TEXT";
                await alterCmd.ExecuteNonQueryAsync();
                Console.WriteLine("    已新增 Remark 欄位");
            }
        }

        // 植入 SystemSetting 系統設定（增量：只補入缺少的 key）
        {
            var settingSeeds = SystemSettingSeed.GetSeedData();
            var existingKeys = await context.SystemSettings
                .Select(s => s.Category + "." + s.Key)
                .ToListAsync();
            var existingSet = new HashSet<string>(existingKeys);

            var newSettings = settingSeeds
                .Where(s => !existingSet.Contains(s.Category + "." + s.Key))
                .ToList();

            if (newSettings.Count > 0)
            {
                var maxId = await context.SystemSettings.AnyAsync()
                    ? await context.SystemSettings.MaxAsync(s => s.Id)
                    : 0;
                foreach (var seed in newSettings)
                {
                    seed.Id = ++maxId;
                }

                context.SystemSettings.AddRange(newSettings);
                await context.SaveChangesAsync();
                Console.WriteLine($"    已補入 {newSettings.Count} 筆系統設定");
            }
            else
            {
                Console.WriteLine($"    系統設定已是最新，無需補入");
            }

            // 同步 Remark 備註（每次執行都從 Seed 更新到 DB）
            var remarkUpdated = 0;
            var allSettings = await context.SystemSettings.ToListAsync();
            foreach (var seed in settingSeeds)
            {
                var existing = allSettings.FirstOrDefault(
                    s => s.Category == seed.Category && s.Key == seed.Key);
                if (existing != null && existing.Remark != seed.Remark)
                {
                    existing.Remark = seed.Remark;
                    remarkUpdated++;
                }
            }
            if (remarkUpdated > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"    已更新 {remarkUpdated} 筆備註");
            }
        }
    }

    // [已移除] InitializeMainDbAsync   — trio240plus_main.db 已廢棄
    // [已移除] InitializeDataDbAsync    — trio240plus_data.db 已廢棄
    // [已移除] InitializeLogDbAsync     — trio240plus_log.db 已廢棄（改用 system_event.db）

    /// <summary>
    /// 設定 SQLite PRAGMA（WAL 模式、外鍵、快取等）
    /// </summary>
    private static async Task SetPragmasAsync(DbContext context)
    {
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        var pragmas = new[]
        {
            "PRAGMA journal_mode = WAL;",           // 允許讀寫並發
            "PRAGMA synchronous = NORMAL;",         // 平衡效能與安全
            "PRAGMA foreign_keys = ON;",            // 啟用外鍵約束
            "PRAGMA cache_size = -2000;",           // 2MB 快取
            "PRAGMA busy_timeout = 5000;",          // 忙碌等待 5 秒
        };

        foreach (var pragma in pragmas)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = pragma;
            await cmd.ExecuteNonQueryAsync();
        }

        Console.WriteLine($"    PRAGMA 設定完成");
    }

    /// <summary>初始化 EventLog DB（system_event.db）— Migration + EventCodeDefinition 種子</summary>
    private static async Task InitializeEventLogDbAsync()
    {
        const string dbFile = "system_event.db";
        var dbPath = GetDatabasePath(dbFile);
        var isNew = !File.Exists(dbPath);

        Console.WriteLine($"  -> 初始化事件日誌庫 ({dbFile})...");

        var options = new DbContextOptionsBuilder<EventLogDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        using var context = new EventLogDbContext(options);
        await context.Database.MigrateAsync();
        Console.WriteLine(isNew ? "    資料庫已建立" : "    資料庫 Migration 完成");

        // Schema 遷移：將舊表名 ErrorDefinition 重命名為 EventCodeDefinition
        var conn = context.Database.GetDbConnection();
        await conn.OpenAsync();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='ErrorDefinition'";
            var oldExists = await cmd.ExecuteScalarAsync();
            if (oldExists != null)
            {
                using var renameCmd = conn.CreateCommand();
                renameCmd.CommandText = "ALTER TABLE ErrorDefinition RENAME TO EventCodeDefinition";
                await renameCmd.ExecuteNonQueryAsync();
                Console.WriteLine("    已將 ErrorDefinition 表重命名為 EventCodeDefinition");
            }
        }

        // 增量植入 + 同步更新 EventCodeDefinition（by Id）
        var seedErrors = EventCodeDefinitionSeed.GetSeedData();
        var existingAll = context.EventCodeDefinitions.ToList();
        var existingIds = existingAll.Select(e => e.Id).ToHashSet();

        // 補入新記錄
        var newErrors = seedErrors.Where(s => !existingIds.Contains(s.Id)).ToList();
        if (newErrors.Count > 0)
        {
            context.EventCodeDefinitions.AddRange(newErrors);
            await context.SaveChangesAsync();
            Console.WriteLine($"    已補入 {newErrors.Count} 筆事件定義");
        }

        // 同步既有記錄的 Code / Severity 等欄位
        var codeUpdated = 0;
        foreach (var seed in seedErrors)
        {
            var existing = existingAll.FirstOrDefault(e => e.Id == seed.Id);
            if (existing != null && existing.Code != seed.Code)
            {
                existing.Code = seed.Code;
                existing.Severity = seed.Severity;
                existing.Title = seed.Title;
                existing.Description = seed.Description;
                existing.Resolution = seed.Resolution;
                existing.UserMessageKey = seed.UserMessageKey;
                existing.UserMessageFallback = seed.UserMessageFallback;
                codeUpdated++;
            }
        }
        if (codeUpdated > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"    已更新 {codeUpdated} 筆事件代碼");
        }

        if (newErrors.Count == 0 && codeUpdated == 0)
        {
            Console.WriteLine("    事件定義已是最新，無需補入");
        }

        await SetPragmasAsync(context);
    }

    /// <summary>初始化正式業務核心庫（main.db）— User 表 + 種子資料</summary>
    private static async Task InitializeAppMainDbAsync(Dictionary<string, string> credentials)
    {
        const string dbFile = "main.db";
        var dbPath = GetDatabasePath(dbFile);
        var isNew = !File.Exists(dbPath);

        Console.WriteLine($"  -> 初始化正式業務核心庫 ({dbFile})...");

        var options = new DbContextOptionsBuilder<AppMainDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        using var context = new AppMainDbContext(options);
        await context.Database.MigrateAsync();
        Console.WriteLine(isNew ? "    資料庫已建立" : "    資料庫 Migration 完成");

        // Schema 遷移：確保 User 表有 IsDeleted / DeletedAt / DeletedBy 欄位
        {
            var conn = context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();

            // 讀取現有欄位
            using var pragmaCmd = conn.CreateCommand();
            pragmaCmd.CommandText = "PRAGMA table_info(User)";
            var existingColumns = new HashSet<string>();
            using (var reader = await pragmaCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    existingColumns.Add(reader.GetString(1));
            }

            // 需要的新欄位（名稱 → ALTER TABLE SQL）
            var requiredColumns = new Dictionary<string, string>
            {
                ["IsDeleted"] = "ALTER TABLE User ADD COLUMN IsDeleted INTEGER NOT NULL DEFAULT 0",
                ["DeletedAt"] = "ALTER TABLE User ADD COLUMN DeletedAt TEXT",
                ["DeletedBy"] = "ALTER TABLE User ADD COLUMN DeletedBy TEXT"
            };

            foreach (var (col, sql) in requiredColumns)
            {
                if (!existingColumns.Contains(col))
                {
                    using var alterCmd = conn.CreateCommand();
                    alterCmd.CommandText = sql;
                    await alterCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"    已新增 User.{col} 欄位");
                }
            }
        }

        // 增量植入 User（按 Id 補入缺少的帳號）
        {
            var users = UserSeed.GetSeedData(credentials, PasswordHasher);
            var existingIds = context.Users.Select(u => u.Id).ToHashSet();
            var newUsers = users.Where(u => !existingIds.Contains(u.Id)).ToList();

            if (newUsers.Count > 0)
            {
                context.Users.AddRange(newUsers);
                await context.SaveChangesAsync();
                Console.WriteLine($"    已補入 {newUsers.Count} 筆使用者帳號");
            }
            else
            {
                Console.WriteLine("    使用者帳號已是最新，無需補入");
            }
        }

        await SetPragmasAsync(context);
    }
}
