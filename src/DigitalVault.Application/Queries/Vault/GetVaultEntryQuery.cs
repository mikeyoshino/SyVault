using DigitalVault.Shared.DTOs.Vault;
using MediatR;

namespace DigitalVault.Application.Queries.Vault;

public class GetVaultEntryQuery : IRequest<VaultEntryDto?>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}
