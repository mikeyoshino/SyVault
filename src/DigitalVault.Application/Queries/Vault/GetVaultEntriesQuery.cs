using DigitalVault.Shared.DTOs.Vault;
using MediatR;

namespace DigitalVault.Application.Queries.Vault;

public class GetVaultEntriesQuery : IRequest<List<VaultEntryDto>>
{
    public Guid UserId { get; set; }
    public string? Category { get; set; }
}
