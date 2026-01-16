using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Enums;
using DigitalVault.Shared.DTOs.DeadManSwitch;
using MediatR;

namespace DigitalVault.Application.Commands.DeadManSwitch;

public class UpdateSwitchCommandHandler : IRequestHandler<UpdateSwitchCommand, DeadManSwitchDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSwitchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeadManSwitchDto> Handle(UpdateSwitchCommand request, CancellationToken cancellationToken)
    {
        var switchEntity = await _context.DeadManSwitches
            .Where(s => s.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (switchEntity == null)
        {
            throw new InvalidOperationException("Dead Man's Switch not found");
        }

        // Update only provided fields
        if (request.CheckInIntervalDays.HasValue)
        {
            switchEntity.CheckInIntervalDays = request.CheckInIntervalDays.Value;
            // Recalculate next check-in due date
            switchEntity.NextCheckInDueDate = switchEntity.LastCheckInAt.AddDays(request.CheckInIntervalDays.Value);
        }

        if (request.GracePeriodDays.HasValue)
        {
            switchEntity.GracePeriodDays = request.GracePeriodDays.Value;
        }

        if (request.ReminderDays != null)
        {
            switchEntity.ReminderDays = request.ReminderDays;
        }

        if (request.NotificationChannels != null)
        {
            switchEntity.NotificationChannels = request.NotificationChannels
                .Select(c => Enum.Parse<NotificationChannel>(c))
                .ToList();
        }

        if (request.EmergencyEmail != null)
        {
            switchEntity.EmergencyEmail = request.EmergencyEmail;
        }

        if (request.EmergencyPhone != null)
        {
            switchEntity.EmergencyPhone = request.EmergencyPhone;
        }

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
