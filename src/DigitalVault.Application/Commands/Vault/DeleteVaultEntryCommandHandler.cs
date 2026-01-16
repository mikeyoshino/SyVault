using DigitalVault.Application.Interfaces;
using MediatR;

namespace DigitalVault.Application.Commands.Vault;

public class DeleteVaultEntryCommandHandler : IRequestHandler<DeleteVaultEntryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteVaultEntryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteVaultEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _context.VaultEntries
            .FirstOrDefaultAsync(v => v.Id == request.Id && v.UserId == request.UserId && !v.IsDeleted, cancellationToken);

        if (entry == null)
        {
            return false;
        }

        // Soft delete
        entry.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
