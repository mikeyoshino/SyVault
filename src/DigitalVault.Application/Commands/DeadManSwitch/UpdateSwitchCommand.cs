using DigitalVault.Shared.DTOs.DeadManSwitch;
using MediatR;

namespace DigitalVault.Application.Commands.DeadManSwitch;

public class UpdateSwitchCommand : IRequest<DeadManSwitchDto>
{
    public Guid UserId { get; set; }
    public int? CheckInIntervalDays { get; set; }
    public int? GracePeriodDays { get; set; }
    public List<int>? ReminderDays { get; set; }
    public List<string>? NotificationChannels { get; set; }
    public string? EmergencyEmail { get; set; }
    public string? EmergencyPhone { get; set; }
}
