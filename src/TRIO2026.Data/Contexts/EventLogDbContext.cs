using Microsoft.EntityFrameworkCore;
using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Contexts;

/// <summary>
/// 系統事件日誌 DbContext — 管理 system_event.db
/// 
/// 包含兩張表：
///   - SystemEvent: 事件日誌記錄
///   - EventCodeDefinition: 預定義事件代碼對照表
/// 
/// 與 trio240plus_log.db 完全獨立。
/// 高頻寫入、低頻查詢，使用 WAL 模式提升並行效能。
/// 
/// 製作者: Office of William
/// </summary>
public class EventLogDbContext : DbContext
{
    public EventLogDbContext(DbContextOptions<EventLogDbContext> options) : base(options) { }

    public DbSet<SystemEvent> SystemEvents { get; set; } = null!;
    public DbSet<EventCodeDefinition> EventCodeDefinitions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── SystemEvent 表 ──
        modelBuilder.Entity<SystemEvent>(entity =>
        {
            entity.ToTable("SystemEvent");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.TimestampLocal).IsRequired();
            entity.Property(e => e.Level).IsRequired().HasMaxLength(16);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Source).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Message).IsRequired();

            entity.Property(e => e.CorrelationId).HasMaxLength(64);
            entity.Property(e => e.ErrorId).HasMaxLength(20);
            entity.Property(e => e.EventCode).HasMaxLength(64);
            entity.Property(e => e.ExceptionType).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(64);
            entity.Property(e => e.SessionId).HasMaxLength(64);
            entity.Property(e => e.MachineName).HasMaxLength(64);
            entity.Property(e => e.AppVersion).HasMaxLength(32);

            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Level);
            entity.HasIndex(e => new { e.Category, e.Source });
            entity.HasIndex(e => e.CorrelationId);
            entity.HasIndex(e => e.ErrorId);
            entity.HasIndex(e => e.EventCode);
            entity.HasIndex(e => e.UserId);
        });

        // ── EventCodeDefinition 對照表 ──
        modelBuilder.Entity<EventCodeDefinition>(entity =>
        {
            entity.ToTable("EventCodeDefinition");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Severity).IsRequired().HasMaxLength(16);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Description).IsRequired();

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Category);
        });
    }
}
