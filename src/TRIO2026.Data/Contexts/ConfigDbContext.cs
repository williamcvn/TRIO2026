using Microsoft.EntityFrameworkCore;
using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Contexts;

/// <summary>
/// 機器配置資料庫上下文 — trio240plus_config.db
/// 包含: SystemConfig, CommandDefinition
/// </summary>
public class ConfigDbContext : DbContext
{
    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();
    public DbSet<CommandDefinition> CommandDefinitions => Set<CommandDefinition>();

    public ConfigDbContext(DbContextOptions<ConfigDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SystemConfig: UNIQUE(Category, Key)
        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.ToTable("SystemConfig");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Category, e.Key }).IsUnique();
            entity.Property(e => e.Category).IsRequired();
            entity.Property(e => e.Key).IsRequired();
            entity.Property(e => e.DataType).IsRequired().HasDefaultValue("string");
            entity.Property(e => e.ModifiedAt).IsRequired();
        });

        // CommandDefinition: Id 為主鍵（0~57）
        modelBuilder.Entity<CommandDefinition>(entity =>
        {
            entity.ToTable("CommandDefinition");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever(); // 手動指定 ID
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Arg0Type).HasDefaultValue(0);
            entity.Property(e => e.Arg1Type).HasDefaultValue(0);
            entity.Property(e => e.Arg2Type).HasDefaultValue(0);
            entity.Property(e => e.Arg3Type).HasDefaultValue(0);
            entity.Property(e => e.Arg4Type).HasDefaultValue(0);
        });
    }
}
