namespace DigitalVault.Shared.DTOs.Auth;

public class EncryptedMasterKeyDto
{
    public string EncryptedMasterKey { get; set; } = string.Empty;
    public string MasterKeySalt { get; set; } = string.Empty;
    public string AuthenticationTag { get; set; } = string.Empty;
}
