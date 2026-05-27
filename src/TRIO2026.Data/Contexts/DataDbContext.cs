using Microsoft.EntityFrameworkCore;
using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Contexts;

/// <summary>
/// 檢測數據資料庫上下文 — data.db
/// 包含: TestRecord, SampleResult, ReportSnapshot
/// 
/// 製作者: Office of William
/// </summary>
public class DataDbContext : DbContext
{
    public DbSet<TestRecord> TestRecords => Set<TestRecord>();
    public DbSet<SampleResult> SampleResults => Set<SampleResult>();
    public DbSet<ReportSnapshot> ReportSnapshots => Set<ReportSnapshot>();

    public DataDbContext(DbContextOptions<DataDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── TestRecord ──
        modelBuilder.Entity<TestRecord>(entity =>
        {
            entity.ToTable("TestRecord");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RunId).IsUnique();
            entity.Property(e => e.RunId).IsRequired();
            entity.Property(e => e.FlowName).IsRequired();
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasDefaultValue("Running");

            // 操作員索引（方便按帳號查詢實驗記錄）
            entity.HasIndex(e => e.OperatorUserId);
            entity.HasIndex(e => e.OperatorUsername);

            // 實驗日期索引
            entity.HasIndex(e => e.ExperimentDate);

            // 報告類型索引
            entity.HasIndex(e => e.ReportType);
        });

        // ── SampleResult: FK → TestRecord (CASCADE DELETE) ──
        modelBuilder.Entity<SampleResult>(entity =>
        {
            entity.ToTable("SampleResult");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TestRecordId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.TestRecord)
                  .WithMany(t => t.SampleResults)
                  .HasForeignKey(e => e.TestRecordId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── ReportSnapshot: FK → TestRecord (CASCADE DELETE) ──
        modelBuilder.Entity<ReportSnapshot>(entity =>
        {
            entity.ToTable("ReportSnapshot");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TestRecordId).IsRequired();
            entity.Property(e => e.ReportType).IsRequired();
            entity.Property(e => e.GeneratedAt).IsRequired();

            // 產生者索引
            entity.HasIndex(e => e.GeneratedByUserId);

            entity.HasOne(e => e.TestRecord)
                  .WithMany(t => t.ReportSnapshots)
                  .HasForeignKey(e => e.TestRecordId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
