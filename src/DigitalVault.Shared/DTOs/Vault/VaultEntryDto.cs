namespace DigitalVault.Shared.DTOs.Vault;

public class VaultEntryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    // Encrypted data
    public byte[] EncryptedDataKey { get; set; } = Array.Empty<byte>();
    public byte[]? EncryptedContent { get; set; }
    public string? BlobStorageUrl { get; set; }
    public byte[] IV { get; set; } = Array.Empty<byte>();
    public string EncryptionAlgorithm { get; set; } = string.Empty;

    public bool IsSharedWithHeirs { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
