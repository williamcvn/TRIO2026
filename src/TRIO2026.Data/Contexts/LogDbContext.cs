using Microsoft.EntityFrameworkCore;
using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Contexts;

/// <summary>
/// 日誌資料庫上下文 — trio240plus_log.db
/// 包含: OperationLog, CommunicationLog
/// </summary>
public class LogDbContext : DbContext
{
    public DbSet<OperationLog> OperationLogs => Set<OperationLog>();
    public DbSet<CommunicationLog> CommunicationLogs => Set<CommunicationLog>();

    public LogDbContext(DbContextOptions<LogDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // OperationLog: 索引 Timestamp + (Category, Level)
        modelBuilder.Entity<OperationLog>(entity =>
        {
            entity.ToTable("OperationLog");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.Category, e.Level });
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Level).IsRequired();
            entity.Property(e => e.Category).IsRequired();
            entity.Property(e => e.Action).IsRequired();
        });

        // CommunicationLog
        modelBuilder.Entity<CommunicationLog>(entity =>
        {
            entity.ToTable("CommunicationLog");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Direction).IsRequired();
            entity.Property(e => e.IsError).HasDefaultValue(0);
        });
    }
}
