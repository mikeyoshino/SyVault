using DigitalVault.Shared.DTOs.DeadManSwitch;
using MediatR;

namespace DigitalVault.Application.Commands.DeadManSwitch;

public class CheckInCommand : IRequest<CheckInResponse>
{
    public Guid UserId { get; set; }
}
