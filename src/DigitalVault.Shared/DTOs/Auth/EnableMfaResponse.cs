namespace DigitalVault.Shared.DTOs.Auth;

public class EnableMfaResponse
{
    public string Secret { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
    public string ManualEntryKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "DigitalVault";
    public string AccountName { get; set; } = string.Empty;
}
