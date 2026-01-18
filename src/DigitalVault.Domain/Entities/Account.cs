using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class Account : BaseEntity
{
    public Guid UserId { get; set; }

    // Encrypted Account Name
    public string EncryptedAccountName { get; set; } = string.Empty;

    // Zero-Knowledge Encryption Fields (unique per account)
    public string EncryptedMasterKey { get; set; } = string.Empty;
    public string MasterKeySalt { get; set; } = string.Empty;
    public string AuthenticationTag { get; set; } = string.Empty;

    // Account Settings
    public bool IsDefault { get; set; }

    // Relationships
    public User User { get; set; } = null!;
    public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<FileAttachment> FileAttachments { get; set; } = new List<FileAttachment>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<AccountCollaborator> Collaborators { get; set; } = new List<AccountCollaborator>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
