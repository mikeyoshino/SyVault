using System;

namespace DigitalVault.Shared.DTOs.Documents;

public class DocumentDto
{
    public Guid Id { get; set; }
    public Guid FamilyMemberId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string S3ObjectKey { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }

    // Encrypted Metadata
    public string EncryptedOriginalFileName { get; set; } = string.Empty;
    public string EncryptedFileExtension { get; set; } = string.Empty;
    public string EncryptedFileSize { get; set; } = string.Empty;
    public string EncryptedMimeType { get; set; } = string.Empty;

    // Encryption Info - Required for upload, maybe not always for list
    public string EncryptionIV { get; set; } = string.Empty;
    public string EncryptionTag { get; set; } = string.Empty;
}
