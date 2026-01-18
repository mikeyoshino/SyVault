using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class DocumentMetadata : BaseEntity
{
    public Guid DocumentId { get; set; }

    // Encrypted Fields (e.g., "IdCardNumber", "ExpiryDate", etc.)
    public string EncryptedFieldName { get; set; } = string.Empty;
    public string EncryptedFieldValue { get; set; } = string.Empty;

    // Relationship
    public Document Document { get; set; } = null!;
}
