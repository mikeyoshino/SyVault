using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Enums;
using DigitalVault.Shared.DTOs.DeadManSwitch;
using MediatR;

namespace DigitalVault.Application.Commands.DeadManSwitch;

public class SetupSwitchCommandHandler : IRequestHandler<SetupSwitchCommand, DeadManSwitchDto>
{
    private readonly IApplicationDbContext _context;

    public SetupSwitchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeadManSwitchDto> Handle(SetupSwitchCommand request, CancellationToken cancellationToken)
    {
        // Check if user exists
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Check if switch already exists for this user
        var existingSwitch = await _context.DeadManSwitches
            .Where(s => s.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingSwitch != null)
        {
            throw new InvalidOperationException("Dead Man's Switch already exists for this user. Use update endpoint instead.");
        }

        // Convert notification channels from strings to enum
        var notificationChannels = request.NotificationChannels?
            .Select(c => Enum.Parse<NotificationChannel>(c))
            .ToList() ?? new List<NotificationChannel> { NotificationChannel.Email };

        // Create Dead Man's Switch
        var switchEntity = new Domain.Entities.DeadManSwitch
        {
            UserId = request.UserId,
            CheckInIntervalDays = request.CheckInIntervalDays,
            GracePeriodDays = request.GracePeriodDays,
            IsActive = true,
            LastCheckInAt = DateTime.UtcNow,
            NextCheckInDueDate = DateTime.UtcNow.AddDays(request.CheckInIntervalDays),
            Status = SwitchStatus.Active,
            ReminderDays = request.ReminderDays ?? new List<int> { 7, 3, 1 },
            NotificationChannels = notificationChannels,
            EmergencyEmail = request.EmergencyEmail,
            EmergencyPhone = request.EmergencyPhone,
            User = user
        };

        _context.DeadManSwitches.Add(switchEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return new DeadManSwitchDto
        {
            Id = switchEntity.Id,
            CheckInIntervalDays = switchEntity.CheckInIntervalDays,
            GracePeriodDays = switchEntity.GracePeriodDays,
            IsActive = switchEntity.IsActive,
            LastCheckInAt = switchEntity.LastCheckInAt,
            NextCheckInDueDate = switchEntity.NextCheckInDueDate,
            Status = switchEntity.Status.ToString(),
            GracePeriodStartedAt = switchEntity.GracePeriodStartedAt,
            TriggeredAt = switchEntity.TriggeredAt,
            ReminderDays = switchEntity.ReminderDays,
            NotificationChannels = switchEntity.NotificationChannels.Select(c => c.ToString()).ToList(),
            EmergencyEmail = switchEntity.EmergencyEmail,
            EmergencyPhone = switchEntity.EmergencyPhone,
            CreatedAt = switchEntity.CreatedAt,
            UpdatedAt = switchEntity.UpdatedAt
        };
    }
}
