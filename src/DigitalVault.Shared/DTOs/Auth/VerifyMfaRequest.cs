namespace DigitalVault.Shared.DTOs.Auth;

public class VerifyMfaRequest
{
    public string Code { get; set; } = string.Empty;
}
