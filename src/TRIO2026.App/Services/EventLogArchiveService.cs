using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TRIO2026.Core;
using TRIO2026.Core.Entities;
using TRIO2026.Data.Contexts;
using TRIO2026.Data.Seeding;

namespace TRIO2026.App.Services;

/// <summary>
/// 事件日誌歸檔服務 — 自動歸檔 + 備份搬移
/// 
/// 流程：
///   1. App 啟動時 CheckAndArchiveAsync()
///   2. 判斷是否需要歸檔（依 archive_interval 設定）
///   3. 將過期資料從 system_event.db 搬到 system_event_{period}.db
///   4. 歸檔 DB 包含 EventCodeDefinition 對照表（方便獨立分析）
///   5. 備份搬移：將已完成的歸檔 DB 搬到 Database_Backup/
/// 
/// 製作者: Office of William
/// </summary>
public class EventLogArchiveService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SystemSettingService _settings;
    private readonly string _databaseDir;
    private readonly string _backupDir;

    public EventLogArchiveService(
        IServiceProvider serviceProvider,
        SystemSettingService settings)
    {
        _serviceProvider = serviceProvider;
        _settings = settings;

        _databaseDir = FindDatabaseDir();
        _backupDir = Path.Combine(Path.GetDirectoryName(_databaseDir)!, "Database_Backup");
    }

    /// <summary>
    /// 啟動檢查 — 判斷是否需要歸檔與備份搬移
    /// </summary>
    public async Task CheckAndArchiveAsync()
    {
        Console.WriteLine("[EventLogArchive] 開始檢查歸檔狀態...");

        try
        {
            var interval = _settings.ArchiveInterval;
            var lastArchive = _settings.LastArchiveDate;
            var now = DateTime.Now.Date;

            // 計算歸檔區間
            var (periodStart, periodEnd, periodLabel) = GetPreviousPeriod(now, interval);

            Console.WriteLine($"[EventLogArchive] 歸檔區間: {interval}, " +
                              $"上次歸檔: {lastArchive?.ToString("yyyy-MM-dd") ?? "從未"}, " +
                              $"待歸檔期間: {periodLabel}");

            // 判斷是否需要歸檔
            if (NeedsArchive(now, lastArchive, interval))
            {
                await ArchivePeriodAsync(periodStart, periodEnd, periodLabel);
                _settings.SetLiveString("EventLog", "last_archive_date", now.ToString("yyyy-MM-dd"));
                Console.WriteLine($"[EventLogArchive] ✅ 歸檔完成: {periodLabel}");

                EventLogService.Instance?.LogInfo("System", "EventLogArchive",
                    ErrorCodes.AppStartup, $"事件日誌歸檔完成: {periodLabel}");
            }
            else
            {
                Console.WriteLine("[EventLogArchive] 尚未到歸檔時間，跳過");
            }

            // 判斷是否需要備份搬移
            await CheckAndBackupAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EventLogArchive] ❌ 歸檔失敗: {ex.Message}");
            EventLogService.Instance?.LogException("System", "EventLogArchive", ex,
                ErrorCodes.EventLogWriteFailure, "歸檔程序失敗");
        }
    }

    /// <summary>判斷是否需要歸檔</summary>
    private static bool NeedsArchive(DateTime now, DateTime? lastArchive, string interval)
    {
        if (lastArchive == null) return true;

        return interval switch
        {
            "weekly" => (now - lastArchive.Value).TotalDays >= 7,
            "quarterly" => (now - lastArchive.Value).TotalDays >= 90,
            _ => now.Month != lastArchive.Value.Month || now.Year != lastArchive.Value.Year // monthly
        };
    }

    /// <summary>取得上一個完整歸檔期間</summary>
    private static (DateTime start, DateTime end, string label) GetPreviousPeriod(
        DateTime now, string interval)
    {
        return interval switch
        {
            "weekly" =>
            (
                now.AddDays(-7 - (int)now.DayOfWeek + 1).Date,
                now.AddDays(-(int)now.DayOfWeek).Date.AddDays(1).AddTicks(-1),
                $"{now.AddDays(-7):yyyyMMdd}_week"
            ),
            "quarterly" =>
            (
                new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1).AddMonths(-3),
                new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1).AddTicks(-1),
                $"{new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1).AddMonths(-3):yyyyMM}_Q"
            ),
            _ => // monthly
            (
                new DateTime(now.Year, now.Month, 1).AddMonths(-1),
                new DateTime(now.Year, now.Month, 1).AddTicks(-1),
                $"{now.AddMonths(-1):yyyyMM}"
            )
        };
    }

    /// <summary>
    /// 將指定期間的事件從 active DB 搬到歸檔 DB
    /// </summary>
    private async Task ArchivePeriodAsync(DateTime periodStart, DateTime periodEnd, string periodLabel)
    {
        var activeDbPath = Path.Combine(_databaseDir, "system_event.db");
        var archiveDbPath = Path.Combine(_databaseDir, $"system_event_{periodLabel}.db");

        if (File.Exists(archiveDbPath))
        {
            Console.WriteLine($"[EventLogArchive] 歸檔檔案已存在: {Path.GetFileName(archiveDbPath)}，跳過");
            return;
        }

        var startStr = periodStart.ToUniversalTime().ToString("o");
        var endStr = periodEnd.ToUniversalTime().ToString("o");

        // 1. 從 active DB 讀取待歸檔事件
        List<SystemEvent> eventsToArchive;
        List<EventCodeDefinition> eventCodeDefs;

        using (var scope = _serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EventLogDbContext>();

            eventsToArchive = await db.SystemEvents
                .Where(e => string.Compare(e.Timestamp, startStr) >= 0 &&
                            string.Compare(e.Timestamp, endStr) <= 0)
                .AsNoTracking()
                .ToListAsync();

            // 附帶 EventCodeDefinition 對照表（方便獨立分析）
            eventCodeDefs = await db.EventCodeDefinitions
                .AsNoTracking()
                .ToListAsync();
        }

        if (eventsToArchive.Count == 0)
        {
            Console.WriteLine($"[EventLogArchive] 期間 {periodLabel} 無事件，跳過歸檔");
            return;
        }

        Console.WriteLine($"[EventLogArchive] 待歸檔事件: {eventsToArchive.Count} 筆");

        // 2. 建立歸檔 DB
        var archiveOptions = new DbContextOptionsBuilder<EventLogDbContext>()
            .UseSqlite($"Data Source={archiveDbPath}")
            .Options;

        using (var archiveDb = new EventLogDbContext(archiveOptions))
        {
            await archiveDb.Database.MigrateAsync();

            // 寫入 EventCodeDefinition 對照表
            if (!archiveDb.EventCodeDefinitions.Any())
            {
                // 重設 Id 以避免衝突
                foreach (var def in eventCodeDefs)
                    archiveDb.Entry(def).State = EntityState.Detached;

                archiveDb.EventCodeDefinitions.AddRange(eventCodeDefs.Select(d => new EventCodeDefinition
                {
                    Code = d.Code,
                    Category = d.Category,
                    Severity = d.Severity,
                    Title = d.Title,
                    Description = d.Description,
                    Resolution = d.Resolution,
                    UserMessageKey = d.UserMessageKey,
                    UserMessageFallback = d.UserMessageFallback
                }));
            }

            // 寫入事件（重設 Id）
            archiveDb.SystemEvents.AddRange(eventsToArchive.Select(e => new SystemEvent
            {
                Timestamp = e.Timestamp,
                TimestampLocal = e.TimestampLocal,
                CorrelationId = e.CorrelationId,
                ErrorId = e.ErrorId,
                Level = e.Level,
                Category = e.Category,
                Source = e.Source,
                EventCode = e.EventCode,
                Message = e.Message,
                Detail = e.Detail,
                ExceptionType = e.ExceptionType,
                StackTrace = e.StackTrace,
                InnerException = e.InnerException,
                UserId = e.UserId,
                UserName = e.UserName,
                SessionId = e.SessionId,
                Tags = e.Tags,
                MachineName = e.MachineName,
                AppVersion = e.AppVersion
            }));

            await archiveDb.SaveChangesAsync();
        }

        Console.WriteLine($"[EventLogArchive] 歸檔 DB 已建立: {Path.GetFileName(archiveDbPath)}");

        // 3. 從 active DB 刪除已歸檔的事件
        using (var scope = _serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EventLogDbContext>();
            var toDelete = await db.SystemEvents
                .Where(e => string.Compare(e.Timestamp, startStr) >= 0 &&
                            string.Compare(e.Timestamp, endStr) <= 0)
                .ToListAsync();

            db.SystemEvents.RemoveRange(toDelete);
            await db.SaveChangesAsync();

            // VACUUM 回收空間
            await db.Database.ExecuteSqlRawAsync("VACUUM;");
        }

        Console.WriteLine($"[EventLogArchive] Active DB 已清理 {eventsToArchive.Count} 筆舊事件");
    }

    /// <summary>
    /// 檢查並執行備份搬移（歸檔 DB → Database_Backup/）
    /// </summary>
    private async Task CheckAndBackupAsync()
    {
        var lastBackup = _settings.LastBackupDate;
        var scheduleDays = _settings.BackupScheduleDays;
        var now = DateTime.Now.Date;

        if (lastBackup != null && (now - lastBackup.Value).TotalDays < scheduleDays)
        {
            Console.WriteLine($"[EventLogArchive] 備份搬移：距上次 {(now - lastBackup.Value).TotalDays:F0} 天，" +
                              $"排程 {scheduleDays} 天，尚未到期");
            return;
        }

        // 掃描歸檔檔案
        var archiveFiles = Directory.GetFiles(_databaseDir, "system_event_*.db")
            .Where(f => !f.EndsWith("system_event.db")) // 排除 active
            .ToList();

        if (archiveFiles.Count == 0)
        {
            Console.WriteLine("[EventLogArchive] 無歸檔檔案需要搬移");
            return;
        }

        // 建立備份目錄
        Directory.CreateDirectory(_backupDir);

        var movedCount = 0;
        foreach (var file in archiveFiles)
        {
            var destPath = Path.Combine(_backupDir, Path.GetFileName(file));
            if (File.Exists(destPath))
            {
                Console.WriteLine($"[EventLogArchive] 備份已存在，跳過: {Path.GetFileName(file)}");
                continue;
            }

            File.Move(file, destPath);
            movedCount++;
            Console.WriteLine($"[EventLogArchive] 已搬移: {Path.GetFileName(file)} → Database_Backup/");
        }

        if (movedCount > 0)
        {
            _settings.SetLiveString("EventLog", "last_backup_date", now.ToString("yyyy-MM-dd"));
            Console.WriteLine($"[EventLogArchive] ✅ 備份搬移完成: {movedCount} 個檔案");

            EventLogService.Instance?.LogInfo("System", "EventLogArchive",
                null, $"備份搬移完成: {movedCount} 個歸檔檔案已搬移至 Database_Backup/");
        }

        await Task.CompletedTask;
    }

    private static string FindDatabaseDir()
    {
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        for (int i = 0; i < 8; i++)
        {
            var dbDir = Path.Combine(dir, "Database");
            if (Directory.Exists(dbDir)) return dbDir;
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        return Path.Combine(@"D:\TRIO2026", "Database");
    }
}
