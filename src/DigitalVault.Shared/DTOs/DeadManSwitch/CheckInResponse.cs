namespace DigitalVault.Shared.DTOs.DeadManSwitch;

public class CheckInResponse
{
    public DateTime CheckInAt { get; set; }
    public DateTime NextCheckInDueDate { get; set; }
    public int DaysUntilNextCheckIn { get; set; }
    public string Message { get; set; } = string.Empty;
}
