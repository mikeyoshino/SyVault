namespace DigitalVault.Application.Interfaces;

public interface IMfaService
{
    /// <summary>
    /// Generate a new MFA secret for TOTP
    /// </summary>
    string GenerateSecret();

    /// <summary>
    /// Generate QR code URL for authenticator apps
    /// </summary>
    string GenerateQrCodeUrl(string secret, string email, string issuer = "DigitalVault");

    /// <summary>
    /// Verify a TOTP code against a secret
    /// </summary>
    bool VerifyCode(string secret, string code);
}
