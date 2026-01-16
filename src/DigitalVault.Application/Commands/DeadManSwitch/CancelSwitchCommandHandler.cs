using DigitalVault.Application.Interfaces;
using MediatR;

namespace DigitalVault.Application.Commands.DeadManSwitch;

public class CancelSwitchCommandHandler : IRequestHandler<CancelSwitchCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CancelSwitchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CancelSwitchCommand request, CancellationToken cancellationToken)
    {
        var switchEntity = await _context.DeadManSwitches
            .Where(s => s.UserId == request.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (switchEntity == null)
        {
            return false;
        }

        switchEntity.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
