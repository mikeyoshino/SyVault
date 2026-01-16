using DigitalVault.Shared.DTOs.DeadManSwitch;
using MediatR;

namespace DigitalVault.Application.Commands.DeadManSwitch;

public class SetupSwitchCommand : IRequest<DeadManSwitchDto>
{
    public Guid UserId { get; set; }
    public int CheckInIntervalDays { get; set; } = 90;
    public int GracePeriodDays { get; set; } = 14;
    public List<int>? ReminderDays { get; set; }
    public List<string>? NotificationChannels { get; set; }
    public string? EmergencyEmail { get; set; }
    public string? EmergencyPhone { get; set; }
}
