namespace DigitalVault.Shared.DTOs.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string? PhoneNumber { get; set; }
    public bool MfaEnabled { get; set; }
    public string SubscriptionTier { get; set; } = string.Empty;
    public DateTime? SubscriptionExpiresAt { get; set; }

    // Key derivation info for client-side encryption
    public byte[] KeyDerivationSalt { get; set; } = Array.Empty<byte>();
    public int KeyDerivationIterations { get; set; }

    // Encrypted master key (encrypted with password-derived key - zero knowledge)
    public string EncryptedMasterKey { get; set; } = string.Empty;
}
