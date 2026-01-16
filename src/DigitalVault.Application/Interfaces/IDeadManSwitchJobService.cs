namespace DigitalVault.Application.Interfaces;

public interface IDeadManSwitchJobService
{
    /// <summary>
    /// Check for overdue switches and send reminders
    /// Runs every hour
    /// </summary>
    Task CheckOverdueSwitchesAsync();

    /// <summary>
    /// Process switches that have entered grace period
    /// and trigger if grace period has expired
    /// Runs every 6 hours
    /// </summary>
    Task ProcessGracePeriodSwitchesAsync();
}
