using DigitalVault.Application.Interfaces;
using OtpNet;

namespace DigitalVault.Infrastructure.Services;

public class MfaService : IMfaService
{
    public string GenerateSecret()
    {
        // Generate a 20-byte (160-bit) secret key
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public string GenerateQrCodeUrl(string secret, string email, string issuer = "DigitalVault")
    {
        // Format: otpauth://totp/Issuer:email?secret=SECRET&issuer=Issuer
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedSecret = Uri.EscapeDataString(secret);

        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={encodedSecret}&issuer={encodedIssuer}";
    }

    public bool VerifyCode(string secret, string code)
    {
        try
        {
            var secretBytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(secretBytes);

            // Verify the code with a window of ±1 period (90 seconds total)
            return totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
        }
        catch
        {
            return false;
        }
    }
}
