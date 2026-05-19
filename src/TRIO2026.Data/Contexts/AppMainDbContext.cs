using Microsoft.EntityFrameworkCore;
using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Contexts;

/// <summary>
/// 正式業務核心 DbContext — 管理 main.db
/// 
/// 取代 trio240plus_main.db（舊版驗證用 DB），
/// 逐步將正式資料表遷移至此。
/// 
/// 第一階段：User 表
/// 後續可加入：Role、Permission、AuditLog 等
/// 
/// 製作者: Office of William
/// </summary>
public class AppMainDbContext : DbContext
{
    public AppMainDbContext(DbContextOptions<AppMainDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(e => e.Id);

            // 身分資訊
            entity.Property(e => e.Username).IsRequired().HasMaxLength(64);
            entity.Property(e => e.DisplayName).HasMaxLength(128);
            entity.Property(e => e.EmployeeId).HasMaxLength(32);
            entity.Property(e => e.Email).HasMaxLength(128);
            entity.Property(e => e.Department).HasMaxLength(64);

            // 安全性
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.LockedUntil).HasMaxLength(32);
            entity.Property(e => e.PasswordChangedAt).HasMaxLength(32);
            entity.Property(e => e.LanguagePreference).HasMaxLength(16);

            // 時間戳記
            entity.Property(e => e.LastLoginAt).HasMaxLength(32);
            entity.Property(e => e.CreatedAt).IsRequired().HasMaxLength(32);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(64);
            entity.Property(e => e.UpdatedAt).HasMaxLength(32);
            entity.Property(e => e.UpdatedBy).HasMaxLength(64);

            // 索引
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.RoleLevel);
            entity.HasIndex(e => e.IsActive);
        });
    }
}
