using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid AccountId { get; set; }
    public Guid UserId { get; set; }

    // Action Info
    public string Action { get; set; } = string.Empty;
    public string? ResourceType { get; set; }
    public Guid? ResourceId { get; set; }

    // Encrypted Details
    public string? EncryptedDetails { get; set; }

    // Metadata
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Relationships
    public Account Account { get; set; } = null!;
    public User User { get; set; } = null!;
}
