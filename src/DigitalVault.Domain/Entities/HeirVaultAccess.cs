using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class HeirVaultAccess : BaseEntity
{
    public Guid HeirId { get; set; }
    public Heir Heir { get; set; } = null!;

    public Guid VaultEntryId { get; set; }
    public VaultEntry VaultEntry { get; set; } = null!;

    // Encrypted DEK for this heir
    public byte[] EncryptedDataKey { get; set; } = Array.Empty<byte>(); // DEK encrypted with heir's public key

    // Access Control
    public bool CanAccess { get; set; } = true;
    public DateTime AccessGrantedAt { get; set; } = DateTime.UtcNow;
}
