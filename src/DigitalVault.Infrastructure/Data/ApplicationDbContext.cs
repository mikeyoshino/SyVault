using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalVault.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<VaultEntry> VaultEntries => Set<VaultEntry>();
    public DbSet<Heir> Heirs => Set<Heir>();
    public DbSet<HeirVaultAccess> HeirVaultAccesses => Set<HeirVaultAccess>();
    public DbSet<DeadManSwitch> DeadManSwitches => Set<DeadManSwitch>();
    public DbSet<SwitchCheckIn> SwitchCheckIns => Set<SwitchCheckIn>();
    public DbSet<SwitchNotification> SwitchNotifications => Set<SwitchNotification>();
    public DbSet<HeirAccessLog> HeirAccessLogs => Set<HeirAccessLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update UpdatedAt timestamp for all modified entities
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Domain.Common.BaseEntity entity)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
