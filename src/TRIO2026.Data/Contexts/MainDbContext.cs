using Microsoft.EntityFrameworkCore;
using TRIO2026.Core.Entities;

namespace TRIO2026.Data.Contexts;

/// <summary>
/// 業務核心資料庫上下文 — trio240plus_main.db
/// 包含: UserAccount, FlowDefinition, FlowStep, FlowMapping, PnidMapping
/// </summary>
public class MainDbContext : DbContext
{
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<RoleDefinition> RoleDefinitions => Set<RoleDefinition>();
    public DbSet<FlowDefinition> FlowDefinitions => Set<FlowDefinition>();
    public DbSet<FlowStep> FlowSteps => Set<FlowStep>();
    public DbSet<FlowMapping> FlowMappings => Set<FlowMapping>();
    public DbSet<PnidMapping> PnidMappings => Set<PnidMapping>();

    public MainDbContext(DbContextOptions<MainDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // UserAccount
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("UserAccount");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.RoleLevel).IsRequired().HasDefaultValue(1);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(1);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.FailedLoginCount).HasDefaultValue(0);

            // FK: UserAccount.RoleLevel → RoleDefinition.Level
            entity.HasOne(e => e.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(e => e.RoleLevel)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // RoleDefinition
        modelBuilder.Entity<RoleDefinition>(entity =>
        {
            entity.ToTable("RoleDefinition");
            entity.HasKey(e => e.Level);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired();
            entity.Property(e => e.DisplayName).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // FlowDefinition
        modelBuilder.Entity<FlowDefinition>(entity =>
        {
            entity.ToTable("FlowDefinition");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FlowName).IsUnique();
            entity.Property(e => e.FlowName).IsRequired();
            entity.Property(e => e.TotalSteps).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(1);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.ModifiedAt).IsRequired();
        });

        // FlowStep: FK → FlowDefinition (CASCADE DELETE)
        modelBuilder.Entity<FlowStep>(entity =>
        {
            entity.ToTable("FlowStep");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.FlowDefinitionId, e.StepOrder });
            entity.Property(e => e.FlowDefinitionId).IsRequired();
            entity.Property(e => e.StepOrder).IsRequired();
            entity.Property(e => e.CommandId).IsRequired();
            entity.Property(e => e.Crc).HasDefaultValue(0);
            entity.Property(e => e.Arg0).HasDefaultValue(0.0);
            entity.Property(e => e.Arg1).HasDefaultValue(0.0);
            entity.Property(e => e.Arg2).HasDefaultValue(0.0);
            entity.Property(e => e.Arg3).HasDefaultValue(0.0);
            entity.Property(e => e.Arg4).HasDefaultValue(0.0);
            entity.Property(e => e.GroupDepth).HasDefaultValue(0);

            entity.HasOne(e => e.FlowDefinition)
                  .WithMany(f => f.Steps)
                  .HasForeignKey(e => e.FlowDefinitionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // FlowMapping — 來源: A09-023 附件4 flowinfo_Flow table
        modelBuilder.Entity<FlowMapping>(entity =>
        {
            entity.ToTable("FlowMapping");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FlowCode).IsUnique();
            entity.Property(e => e.FlowCode).IsRequired();
            entity.Property(e => e.BuiltInFlowName).IsRequired();
        });

        // PnidMapping
        modelBuilder.Entity<PnidMapping>(entity =>
        {
            entity.ToTable("PnidMapping");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PnidCode).IsUnique();
            entity.Property(e => e.PnidCode).IsRequired();
        });
    }
}
