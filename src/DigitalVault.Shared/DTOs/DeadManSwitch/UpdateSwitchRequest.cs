namespace DigitalVault.Shared.DTOs.DeadManSwitch;

public class UpdateSwitchRequest
{
    public int? CheckInIntervalDays { get; set; }
    public int? GracePeriodDays { get; set; }
    public List<int>? ReminderDays { get; set; }
    public List<string>? NotificationChannels { get; set; }
    public string? EmergencyEmail { get; set; }
    public string? EmergencyPhone { get; set; }
}
