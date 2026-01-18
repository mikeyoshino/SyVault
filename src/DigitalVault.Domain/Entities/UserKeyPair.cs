using DigitalVault.Domain.Common;

namespace DigitalVault.Domain.Entities;

public class UserKeyPair : BaseEntity
{
    public Guid UserId { get; set; }

    // RSA Key Pair for sharing
    public string PublicKey { get; set; } = string.Empty;
    public string EncryptedPrivateKey { get; set; } = string.Empty;
    public string PrivateKeySalt { get; set; } = string.Empty;

    // Key Info
    public string KeyAlgorithm { get; set; } = "RSA-4096";
    public bool IsActive { get; set; } = true;

    // Relationship
    public User User { get; set; } = null!;
}
