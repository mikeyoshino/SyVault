using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class FileAttachment : BaseEntity
{
    public Guid FamilyMemberId { get; set; }
    public Guid AccountId { get; set; }

    // AWS S3 Info
    public string S3BucketName { get; set; } = string.Empty;
    public string S3ObjectKey { get; set; } = string.Empty;
    public string S3Region { get; set; } = string.Empty;

    // Encrypted Metadata
    public string EncryptedFileName { get; set; } = string.Empty;
    public string EncryptedFileExtension { get; set; } = string.Empty;
    public string EncryptedFileSize { get; set; } = string.Empty;
    public string EncryptedMimeType { get; set; } = string.Empty;
    public string? EncryptedDescription { get; set; }

    // Encryption Info
    public string EncryptionIV { get; set; } = string.Empty;
    public string EncryptionTag { get; set; } = string.Empty;

    // Folder Organization
    public string? EncryptedFolderPath { get; set; }

    // Timestamp
    public DateTime UploadedAt { get; set; }

    // Relationships
    public FamilyMember FamilyMember { get; set; } = null!;
    public Account Account { get; set; } = null!;
}
