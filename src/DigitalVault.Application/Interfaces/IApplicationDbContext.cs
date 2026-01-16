using DigitalVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalVault.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<VaultEntry> VaultEntries { get; }
    DbSet<Heir> Heirs { get; }
    DbSet<HeirVaultAccess> HeirVaultAccesses { get; }
    DbSet<DeadManSwitch> DeadManSwitches { get; }
    DbSet<SwitchCheckIn> SwitchCheckIns { get; }
    DbSet<SwitchNotification> SwitchNotifications { get; }
    DbSet<HeirAccessLog> HeirAccessLogs { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
