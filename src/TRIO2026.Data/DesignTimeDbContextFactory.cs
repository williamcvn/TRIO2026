using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TRIO2026.Data.Contexts;

namespace TRIO2026.Data;

/// <summary>
/// Design-Time Factory — 供 EF Core CLI (dotnet ef) 使用
/// 在 CLI 環境下，DI 容器不可用，需要此 Factory 建立 DbContext
/// </summary>

public class SystemConfigDbContextFactory : IDesignTimeDbContextFactory<SystemConfigDbContext>
{
    public SystemConfigDbContext CreateDbContext(string[] args)
    {
        var dbPath = Path.Combine(FindDatabaseDir(), "system_config.db");
        var options = new DbContextOptionsBuilder<SystemConfigDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
        return new SystemConfigDbContext(options);
    }

    private static string FindDatabaseDir()
    {
        var dir = Directory.GetCurrentDirectory();
        for (int i = 0; i < 8; i++)
        {
            var dbDir = Path.Combine(dir, "Database");
            if (Directory.Exists(dbDir)) return dbDir;
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        return @"D:\TRIO2026\Database";
    }
}

public class LogDbContextFactory : IDesignTimeDbContextFactory<LogDbContext>
{
    public LogDbContext CreateDbContext(string[] args)
    {
        var dbPath = Path.Combine(FindDatabaseDir(), "trio240plus_log.db");
        var options = new DbContextOptionsBuilder<LogDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
        return new LogDbContext(options);
    }

    private static string FindDatabaseDir()
    {
        var dir = Directory.GetCurrentDirectory();
        for (int i = 0; i < 8; i++)
        {
            var dbDir = Path.Combine(dir, "Database");
            if (Directory.Exists(dbDir)) return dbDir;
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        return @"D:\TRIO2026\Database";
    }
}

public class EventLogDbContextFactory : IDesignTimeDbContextFactory<EventLogDbContext>
{
    public EventLogDbContext CreateDbContext(string[] args)
    {
        var dbPath = Path.Combine(FindDatabaseDir(), "system_event.db");
        var options = new DbContextOptionsBuilder<EventLogDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
        return new EventLogDbContext(options);
    }

    private static string FindDatabaseDir()
    {
        var dir = Directory.GetCurrentDirectory();
        for (int i = 0; i < 8; i++)
        {
            var dbDir = Path.Combine(dir, "Database");
            if (Directory.Exists(dbDir)) return dbDir;
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        return @"D:\TRIO2026\Database";
    }
}

public class AppMainDbContextFactory : IDesignTimeDbContextFactory<AppMainDbContext>
{
    public AppMainDbContext CreateDbContext(string[] args)
    {
        var dbPath = Path.Combine(FindDatabaseDir(), "main.db");
        var options = new DbContextOptionsBuilder<AppMainDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
        return new AppMainDbContext(options);
    }

    private static string FindDatabaseDir()
    {
        var dir = Directory.GetCurrentDirectory();
        for (int i = 0; i < 8; i++)
        {
            var dbDir = Path.Combine(dir, "Database");
            if (Directory.Exists(dbDir)) return dbDir;
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        return @"D:\TRIO2026\Database";
    }
}
