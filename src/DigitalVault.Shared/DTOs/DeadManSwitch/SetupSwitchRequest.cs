namespace DigitalVault.Shared.DTOs.DeadManSwitch;

public class SetupSwitchRequest
{
    public int CheckInIntervalDays { get; set; } = 90; // Default: 90 days
    public int GracePeriodDays { get; set; } = 14; // Default: 14 days
    public List<int>? ReminderDays { get; set; } // e.g., [7, 3, 1] - remind 7, 3, and 1 days before due
    public List<string>? NotificationChannels { get; set; } // e.g., ["Email", "SMS"]
    public string? EmergencyEmail { get; set; }
    public string? EmergencyPhone { get; set; }
}
