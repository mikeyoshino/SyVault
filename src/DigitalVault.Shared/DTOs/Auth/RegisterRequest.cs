namespace DigitalVault.Shared.DTOs.Auth;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    // Key derivation info (generated client-side)
    public byte[] KeyDerivationSalt { get; set; } = Array.Empty<byte>();
    public int KeyDerivationIterations { get; set; } = 100000;

    // Encrypted master key (encrypted client-side with password-derived key)
    public string EncryptedMasterKey { get; set; } = string.Empty;
}
