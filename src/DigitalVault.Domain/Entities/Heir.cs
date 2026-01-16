using DigitalVault.Domain.Common;
using DigitalVault.Domain.Enums;

namespace DigitalVault.Domain.Entities;

public class Heir : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Heir Information
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty; // Encrypted
    public string? Relationship { get; set; } // 'spouse', 'child', 'sibling', etc.

    // Verification
    public bool IsVerified { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime? VerificationExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }

    // Access Keys (for Zero-Knowledge)
    public byte[] PublicKey { get; set; } = Array.Empty<byte>(); // Heir's RSA public key
    public byte[]? EncryptedPrivateKey { get; set; } // Encrypted with heir's password

    // Permissions
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Full;
    public List<string> CanAccessCategories { get; set; } = new();

    // Relationships
    public ICollection<HeirVaultAccess> VaultAccesses { get; set; } = new List<HeirVaultAccess>();
    public ICollection<HeirAccessLog> AccessLogs { get; set; } = new List<HeirAccessLog>();

    // Metadata
    public bool IsDeleted { get; set; }
}
