using MediatR;

namespace DigitalVault.Application.Commands.Heir;

public class RemoveHeirCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}
