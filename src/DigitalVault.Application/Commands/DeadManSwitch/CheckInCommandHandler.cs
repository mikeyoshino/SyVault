using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Enums;
using DigitalVault.Shared.DTOs.DeadManSwitch;
using MediatR;

namespace DigitalVault.Application.Commands.DeadManSwitch;

public class CheckInCommandHandler : IRequestHandler<CheckInCommand, CheckInResponse>
{
    private readonly IApplicationDbContext _context;

    public CheckInCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CheckInResponse> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var switchEntity = await _context.DeadManSwitches
            .Where(s => s.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (switchEntity == null)
        {
            throw new InvalidOperationException("Dead Man's Switch not found. Please setup the switch first.");
        }

        if (!switchEntity.IsActive)
        {
            throw new InvalidOperationException("Dead Man's Switch is inactive. Please activate it first.");
        }

        // Record check-in
        var checkIn = new Domain.Entities.SwitchCheckIn
        {
            SwitchId = switchEntity.Id,
            UserId = request.UserId,
            CheckInAt = DateTime.UtcNow,
            CheckInMethod = "Manual" // Could be "Email", "SMS", "Web", etc.
        };

        _context.SwitchCheckIns.Add(checkIn);

        // Update switch status
        switchEntity.LastCheckInAt = DateTime.UtcNow;
        switchEntity.NextCheckInDueDate = DateTime.UtcNow.AddDays(switchEntity.CheckInIntervalDays);

        // Reset status if it was in grace period
        if (switchEntity.Status == SwitchStatus.GracePeriod)
        {
            switchEntity.Status = SwitchStatus.Active;
            switchEntity.GracePeriodStartedAt = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var daysUntilNext = (int)(switchEntity.NextCheckInDueDate - DateTime.UtcNow).TotalDays;

        return new CheckInResponse
        {
            CheckInAt = checkIn.CheckInAt,
            NextCheckInDueDate = switchEntity.NextCheckInDueDate,
            DaysUntilNextCheckIn = daysUntilNext,
            Message = $"Check-in successful! Your next check-in is due in {daysUntilNext} days."
        };
    }
}
