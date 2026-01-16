using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class HeirAccessLog : BaseEntity
{
    public Guid HeirId { get; set; }
    public Heir Heir { get; set; } = null!;

    public Guid VaultEntryId { get; set; }
    public VaultEntry VaultEntry { get; set; } = null!;

    public Guid UserId { get; set; } // Owner
    public User User { get; set; } = null!;

    public string AccessType { get; set; } = string.Empty; // 'view', 'download', 'decrypt'
    public DateTime AccessedAt { get; set; } = DateTime.UtcNow;

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Metadata
    public bool WasSuccessful { get; set; } = true;
    public string? FailureReason { get; set; }
}
