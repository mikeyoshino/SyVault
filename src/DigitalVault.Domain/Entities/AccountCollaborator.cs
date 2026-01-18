using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class AccountCollaborator : BaseEntity
{
    public Guid AccountId { get; set; }
    public Guid UserId { get; set; }
    public Guid InvitedByUserId { get; set; }

    // Permission Level
    public string PermissionLevel { get; set; } = string.Empty; // 'Viewer', 'Editor', 'Admin'

    // Encrypted Master Key for Collaborator
    public string EncryptedMasterKeyForCollaborator { get; set; } = string.Empty;

    // Invitation Status
    public string InvitationStatus { get; set; } = "Pending"; // 'Pending', 'Accepted', 'Declined'
    public string? InvitationToken { get; set; }
    public DateTime? InvitationExpiresAt { get; set; }

    // Timestamps
    public DateTime InvitedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }

    // Status
    public bool IsActive { get; set; } = true;
    public DateTime? RevokedAt { get; set; }
    public Guid? RevokedByUserId { get; set; }

    // Relationships
    public Account Account { get; set; } = null!;
    public User User { get; set; } = null!;
    public User InvitedBy { get; set; } = null!;
    public User? RevokedBy { get; set; }
}
