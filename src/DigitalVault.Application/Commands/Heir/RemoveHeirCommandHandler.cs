using DigitalVault.Application.Interfaces;
using MediatR;

namespace DigitalVault.Application.Commands.Heir;

public class RemoveHeirCommandHandler : IRequestHandler<RemoveHeirCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RemoveHeirCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RemoveHeirCommand request, CancellationToken cancellationToken)
    {
        var heir = await _context.Heirs
            .Where(h => h.Id == request.Id && h.UserId == request.UserId && !h.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (heir == null)
        {
            return false;
        }

        // Soft delete
        heir.IsDeleted = true;

        // Also remove all heir vault accesses
        var heirAccesses = await _context.HeirVaultAccesses
            .Where(hva => hva.HeirId == request.Id)
            .ToListAsync(cancellationToken);

        _context.HeirVaultAccesses.RemoveRange(heirAccesses);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
