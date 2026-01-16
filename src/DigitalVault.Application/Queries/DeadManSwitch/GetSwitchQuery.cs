using DigitalVault.Shared.DTOs.DeadManSwitch;
using MediatR;

namespace DigitalVault.Application.Queries.DeadManSwitch;

public class GetSwitchQuery : IRequest<DeadManSwitchDto?>
{
    public Guid UserId { get; set; }
}
