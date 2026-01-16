namespace DigitalVault.Shared.DTOs.Vault;

public class UpdateVaultEntryRequest
{
    public string Title { get; set; } = string.Empty;
    public byte[] EncryptedDataKey { get; set; } = Array.Empty<byte>();
    public byte[]? EncryptedContent { get; set; }
    public byte[] IV { get; set; } = Array.Empty<byte>();
    public bool IsSharedWithHeirs { get; set; }
}
