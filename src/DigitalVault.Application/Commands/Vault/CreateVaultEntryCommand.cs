using DigitalVault.Shared.DTOs.Vault;
using MediatR;

namespace DigitalVault.Application.Commands.Vault;

public class CreateVaultEntryCommand : IRequest<VaultEntryDto>
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public byte[] EncryptedDataKey { get; set; } = Array.Empty<byte>();
    public byte[]? EncryptedContent { get; set; }
    public byte[] IV { get; set; } = Array.Empty<byte>();
    public bool IsSharedWithHeirs { get; set; } = true;
}
