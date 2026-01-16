namespace DigitalVault.Shared.DTOs.DeadManSwitch;

public class DeadManSwitchDto
{
    public Guid Id { get; set; }
    public int CheckInIntervalDays { get; set; }
    public int GracePeriodDays { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastCheckInAt { get; set; }
    public DateTime NextCheckInDueDate { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? GracePeriodStartedAt { get; set; }
    public DateTime? TriggeredAt { get; set; }
    public List<int> ReminderDays { get; set; } = new();
    public List<string> NotificationChannels { get; set; } = new();
    public string? EmergencyEmail { get; set; }
    public string? EmergencyPhone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
