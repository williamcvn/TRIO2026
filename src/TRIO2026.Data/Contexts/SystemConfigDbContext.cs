using Microsoft.EntityFrameworkCore;
using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Contexts;

/// <summary>
/// 系統配置資料庫上下文 — system_config.db
/// 包含: UvTimerOption, LocalizedString, SystemSetting
/// 
/// 此 DB 獨立於 trio240plus_config.db，專門存放：
///   - UV 照射時間選項管理
///   - 多語系 UI 字串資源
///   - 系統級 Key-Value 設定
/// </summary>
public class SystemConfigDbContext : DbContext
{
    public DbSet<UvTimerOption> UvTimerOptions => Set<UvTimerOption>();
    public DbSet<LocalizedString> LocalizedStrings => Set<LocalizedString>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public SystemConfigDbContext(DbContextOptions<SystemConfigDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // UvTimerOption: UV 照射時間選項
        modelBuilder.Entity<UvTimerOption>(entity =>
        {
            entity.ToTable("UvTimerOption");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DurationSeconds).IsUnique();   // 秒數不可重複
            entity.Property(e => e.DurationSeconds).IsRequired();
            entity.Property(e => e.DisplayLabel).IsRequired();
            entity.Property(e => e.IsEnabled).IsRequired().HasDefaultValue(1);
            entity.Property(e => e.IsDefault).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.SortOrder).IsRequired().HasDefaultValue(0);
        });

        // LocalizedString: 多語系字串資源
        modelBuilder.Entity<LocalizedString>(entity =>
        {
            entity.ToTable("LocalizedString");
            entity.HasKey(e => e.Id);
            // 同一模組 + 同一 Key + 同一語系 = 唯一
            entity.HasIndex(e => new { e.Module, e.ResourceKey, e.LanguageCode }).IsUnique();
            entity.Property(e => e.Module).IsRequired();
            entity.Property(e => e.ResourceKey).IsRequired();
            entity.Property(e => e.LanguageCode).IsRequired().HasDefaultValue("en");
            entity.Property(e => e.Value).IsRequired();
            // 為語系查詢建立索引（載入指定語系的所有字串）
            entity.HasIndex(e => e.LanguageCode);
            // 為模組查詢建立索引（按模組載入）
            entity.HasIndex(e => new { e.Module, e.LanguageCode });
        });

        // SystemSetting: 系統級 Key-Value 設定
        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.ToTable("SystemSetting");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Category, e.Key }).IsUnique();
            entity.Property(e => e.Category).IsRequired();
            entity.Property(e => e.Key).IsRequired();
            entity.Property(e => e.Value).IsRequired().HasDefaultValue("");
        });
    }
}
