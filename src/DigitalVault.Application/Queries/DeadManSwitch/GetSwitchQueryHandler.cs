using DigitalVault.Application.Interfaces;
using DigitalVault.Shared.DTOs.DeadManSwitch;
using MediatR;

namespace DigitalVault.Application.Queries.DeadManSwitch;

public class GetSwitchQueryHandler : IRequestHandler<GetSwitchQuery, DeadManSwitchDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSwitchQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeadManSwitchDto?> Handle(GetSwitchQuery request, CancellationToken cancellationToken)
    {
        var switchEntity = await _context.DeadManSwitches
            .Where(s => s.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (switchEntity == null)
        {
            return null;
        }

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
