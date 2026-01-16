using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DigitalVault.Infrastructure.Services;

public class DeadManSwitchJobService : IDeadManSwitchJobService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeadManSwitchJobService> _logger;

    public DeadManSwitchJobService(
        IApplicationDbContext context,
        ILogger<DeadManSwitchJobService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CheckOverdueSwitchesAsync()
    {
        _logger.LogInformation("Starting CheckOverdueSwitchesAsync job");

        try
        {
            var now = DateTime.UtcNow;

            // Get all active switches
            var switches = await _context.DeadManSwitches
                .Where(s => s.IsActive && s.Status == SwitchStatus.Active)
                .ToListAsync();

            _logger.LogInformation($"Found {switches.Count} active switches to check");

            foreach (var switchEntity in switches)
            {
                var daysUntilDue = (switchEntity.NextCheckInDueDate - now).TotalDays;

                // Check if reminder should be sent
                foreach (var reminderDay in switchEntity.ReminderDays.OrderByDescending(d => d))
                {
                    if (daysUntilDue <= reminderDay && daysUntilDue > 0)
                    {
                        // Check if reminder already sent for this day
                        var reminderExists = await _context.SwitchNotifications
                            .Where(n => n.SwitchId == switchEntity.Id
                                && n.NotificationType == $"Reminder_{reminderDay}Days"
                                && n.SentAt > now.AddDays(-reminderDay))
                            .AnyAsync();

                        if (!reminderExists)
                        {
                            await SendReminderNotificationAsync(switchEntity, reminderDay);
                        }
                    }
                }

                // Check if switch is overdue (grace period should start)
                if (now > switchEntity.NextCheckInDueDate && switchEntity.Status == SwitchStatus.Active)
                {
                    _logger.LogWarning($"Switch {switchEntity.Id} is overdue. Starting grace period.");

                    switchEntity.Status = SwitchStatus.GracePeriod;
                    switchEntity.GracePeriodStartedAt = now;

                    await SendOverdueNotificationAsync(switchEntity);
                }
            }

            await _context.SaveChangesAsync(default);
            _logger.LogInformation("CheckOverdueSwitchesAsync job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CheckOverdueSwitchesAsync");
            throw;
        }
    }

    public async Task ProcessGracePeriodSwitchesAsync()
    {
        _logger.LogInformation("Starting ProcessGracePeriodSwitchesAsync job");

        try
        {
            var now = DateTime.UtcNow;

            // Get all switches in grace period
            var switchesInGrace = await _context.DeadManSwitches
                .Include(s => s.User)
                    .ThenInclude(u => u.Heirs.Where(h => h.IsVerified && !h.IsDeleted))
                .Include(s => s.User)
                    .ThenInclude(u => u.VaultEntries.Where(v => v.IsSharedWithHeirs && !v.IsDeleted))
                .Where(s => s.IsActive && s.Status == SwitchStatus.GracePeriod)
                .ToListAsync();

            _logger.LogInformation($"Found {switchesInGrace.Count} switches in grace period");

            foreach (var switchEntity in switchesInGrace)
            {
                if (switchEntity.GracePeriodStartedAt.HasValue)
                {
                    var gracePeriodEnd = switchEntity.GracePeriodStartedAt.Value.AddDays(switchEntity.GracePeriodDays);

                    if (now >= gracePeriodEnd)
                    {
                        _logger.LogWarning($"Switch {switchEntity.Id} grace period expired. Triggering switch.");

                        // Trigger the switch
                        switchEntity.Status = SwitchStatus.Triggered;
                        switchEntity.TriggeredAt = now;

                        // Notify heirs
                        await NotifyHeirsAsync(switchEntity);
                    }
                }
            }

            await _context.SaveChangesAsync(default);
            _logger.LogInformation("ProcessGracePeriodSwitchesAsync job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessGracePeriodSwitchesAsync");
            throw;
        }
    }

    private async Task SendReminderNotificationAsync(Domain.Entities.DeadManSwitch switchEntity, int daysRemaining)
    {
        _logger.LogInformation($"Sending {daysRemaining}-day reminder for switch {switchEntity.Id}");

        var notification = new Domain.Entities.SwitchNotification
        {
            SwitchId = switchEntity.Id,
            NotificationType = $"Reminder_{daysRemaining}Days",
            Channel = "Email", // TODO: Support multiple channels
            SentAt = DateTime.UtcNow,
            Status = "Sent",
            Subject = $"Check-in Reminder: {daysRemaining} days remaining",
            Body = $"Your Dead Man's Switch check-in is due in {daysRemaining} days. " +
                   $"Please check in before {switchEntity.NextCheckInDueDate:yyyy-MM-dd HH:mm} UTC to avoid triggering the switch."
        };

        _context.SwitchNotifications.Add(notification);

        // TODO: Actually send email via email service
        _logger.LogInformation($"Reminder notification created for switch {switchEntity.Id}");
    }

    private async Task SendOverdueNotificationAsync(Domain.Entities.DeadManSwitch switchEntity)
    {
        _logger.LogWarning($"Sending overdue notification for switch {switchEntity.Id}");

        var notification = new Domain.Entities.SwitchNotification
        {
            SwitchId = switchEntity.Id,
            NotificationType = "Overdue",
            Channel = "Email",
            SentAt = DateTime.UtcNow,
            Status = "Sent",
            Subject = "URGENT: Dead Man's Switch Overdue - Grace Period Started",
            Body = $"Your Dead Man's Switch check-in was overdue. " +
                   $"A {switchEntity.GracePeriodDays}-day grace period has started. " +
                   $"Please check in immediately to avoid triggering the switch and notifying your heirs."
        };

        _context.SwitchNotifications.Add(notification);

        _logger.LogInformation($"Overdue notification created for switch {switchEntity.Id}");
    }

    private async Task NotifyHeirsAsync(Domain.Entities.DeadManSwitch switchEntity)
    {
        _logger.LogWarning($"Notifying heirs for triggered switch {switchEntity.Id}");

        var heirs = switchEntity.User.Heirs.Where(h => h.IsVerified && !h.IsDeleted).ToList();

        _logger.LogInformation($"Found {heirs.Count} verified heirs to notify");

        foreach (var heir in heirs)
        {
            var notification = new Domain.Entities.SwitchNotification
            {
                SwitchId = switchEntity.Id,
                NotificationType = "SwitchTriggered",
                Channel = "Email",
                SentAt = DateTime.UtcNow,
                Status = "Sent",
                Subject = "Digital Inheritance Notification",
                Body = $"Dear {heir.FullName},\n\n" +
                       $"This is an automated notification from the Digital Vault system.\n\n" +
                       $"{switchEntity.User.Email} has designated you as an heir. " +
                       $"Their Dead Man's Switch has been triggered after {switchEntity.CheckInIntervalDays + switchEntity.GracePeriodDays} days without check-in.\n\n" +
                       $"You now have access to the vault entries they shared with you. " +
                       $"Please log in to the Digital Vault to access your inheritance.\n\n" +
                       $"Number of shared vault entries: {switchEntity.User.VaultEntries.Count(v => v.IsSharedWithHeirs && !v.IsDeleted)}"
            };

            _context.SwitchNotifications.Add(notification);

            _logger.LogInformation($"Heir notification created for heir {heir.Id} ({heir.Email})");

            // TODO: Actually send email via email service
        }

        _logger.LogInformation($"All heir notifications created for switch {switchEntity.Id}");
    }
}
