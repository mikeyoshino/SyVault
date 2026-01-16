using DigitalVault.Domain.Common;
using DigitalVault.Domain.Enums;

namespace DigitalVault.Domain.Entities;

public class VaultEntry : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Entry Info (title is encrypted on client)
    public string Title { get; set; } = string.Empty;
    public VaultCategory Category { get; set; }

    // Encrypted Data
    public byte[] EncryptedDataKey { get; set; } = Array.Empty<byte>(); // DEK encrypted with master key
    public byte[]? EncryptedContent { get; set; } // Small data (passwords, notes)
    public string? BlobStorageUrl { get; set; } // For large files
    public string? BlobStorageKey { get; set; }

    // Encryption Metadata
    public byte[] IV { get; set; } = Array.Empty<byte>(); // Initialization vector
    public string EncryptionAlgorithm { get; set; } = "AES-256-GCM";

    // Access Control
    public bool IsSharedWithHeirs { get; set; } = true;

    // Relationships
    public ICollection<HeirVaultAccess> HeirAccesses { get; set; } = new List<HeirVaultAccess>();

    // Metadata
    public bool IsDeleted { get; set; }
}
