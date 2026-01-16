using MediatR;

namespace DigitalVault.Application.Commands.Vault;

public class DeleteVaultEntryCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}
