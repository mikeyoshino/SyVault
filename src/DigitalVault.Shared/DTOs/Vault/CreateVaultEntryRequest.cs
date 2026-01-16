namespace DigitalVault.Shared.DTOs.Vault;

public class CreateVaultEntryRequest
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    // Client sends encrypted data
    public byte[] EncryptedDataKey { get; set; } = Array.Empty<byte>();
    public byte[]? EncryptedContent { get; set; }
    public byte[] IV { get; set; } = Array.Empty<byte>();

    public bool IsSharedWithHeirs { get; set; } = true;
}
