using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string Action { get; set; } = string.Empty; // 'vault.create', 'vault.update', 'heir.add', etc.
    public string? EntityType { get; set; } // 'VaultEntry', 'Heir', 'DeadManSwitch'
    public Guid? EntityId { get; set; }

    // Details (stored as JSON)
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    // Context
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
