using DigitalVault.Domain.Common;
using DigitalVault.Domain.Enums;

namespace DigitalVault.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneVerified { get; set; }

    // Authentication (account password, NOT master encryption key)
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;

    // MFA
    public bool MfaEnabled { get; set; }
    public string? MfaSecret { get; set; }

    // Key Derivation Info (sent to client for key derivation)
    public byte[] KeyDerivationSalt { get; set; } = Array.Empty<byte>();
    public int KeyDerivationIterations { get; set; } = 100000;

    // Master Encryption Key (encrypted with password-derived key - zero knowledge)
    // Client encrypts this before sending, server never knows the plaintext master key
    public string EncryptedMasterKey { get; set; } = string.Empty;

    // Subscription
    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;
    public DateTime? SubscriptionExpiresAt { get; set; }

    // Relationships
    public ICollection<VaultEntry> VaultEntries { get; set; } = new List<VaultEntry>();
    public ICollection<Heir> Heirs { get; set; } = new List<Heir>();
    public DeadManSwitch? DeadManSwitch { get; set; }

    // Metadata
    public DateTime? LastLoginAt { get; set; }
    public bool IsDeleted { get; set; }
}
