using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class Document : BaseEntity
{
    public Guid FamilyMemberId { get; set; }
    public Guid AccountId { get; set; }

    // Document Type
    public string DocumentType { get; set; } = string.Empty; // 'IdCard', 'DriverLicense', 'Passport', etc.

    // AWS S3 Info
    public string S3BucketName { get; set; } = string.Empty;
    public string S3ObjectKey { get; set; } = string.Empty;
    public string S3Region { get; set; } = string.Empty;

    // Encrypted Metadata
    public string EncryptedOriginalFileName { get; set; } = string.Empty;
    public string EncryptedFileExtension { get; set; } = string.Empty;
    public string EncryptedFileSize { get; set; } = string.Empty;
    public string EncryptedMimeType { get; set; } = string.Empty;

    // Encryption Info
    public string EncryptionIV { get; set; } = string.Empty;
    public string EncryptionTag { get; set; } = string.Empty;

    // Timestamps
    public DateTime UploadedAt { get; set; }

    // Relationships
    public FamilyMember FamilyMember { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public ICollection<DocumentMetadata> Metadata { get; set; } = new List<DocumentMetadata>();
}
