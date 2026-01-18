using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class FamilyMember : BaseEntity
{
    public Guid AccountId { get; set; }

    // Encrypted Data
    public string EncryptedFirstName { get; set; } = string.Empty;
    public string EncryptedLastName { get; set; } = string.Empty;
    public string? EncryptedRelationship { get; set; }
    public string? EncryptedDateOfBirth { get; set; }
    public string? EncryptedPhoneNumber { get; set; }
    public string? EncryptedEmail { get; set; }
    public string? EncryptedNotes { get; set; }

    // Metadata (not encrypted - for UI display only)
    public string? AvatarColor { get; set; }
    public string? InitialsPlainText { get; set; }

    // Relationships
    public Account Account { get; set; } = null!;
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<FileAttachment> FileAttachments { get; set; } = new List<FileAttachment>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
}
