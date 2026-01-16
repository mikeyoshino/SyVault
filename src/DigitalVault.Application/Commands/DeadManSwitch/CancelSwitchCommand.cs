using MediatR;

namespace DigitalVault.Application.Commands.DeadManSwitch;

public class CancelSwitchCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
}
