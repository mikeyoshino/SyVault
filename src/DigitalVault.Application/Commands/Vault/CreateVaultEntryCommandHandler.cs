using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Entities;
using DigitalVault.Domain.Enums;
using DigitalVault.Shared.DTOs.Vault;
using MediatR;

namespace DigitalVault.Application.Commands.Vault;

public class CreateVaultEntryCommandHandler : IRequestHandler<CreateVaultEntryCommand, VaultEntryDto>
{
    private readonly IApplicationDbContext _context;

    public CreateVaultEntryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VaultEntryDto> Handle(CreateVaultEntryCommand request, CancellationToken cancellationToken)
    {
        // Verify user exists
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Check subscription limits (Free tier: max 3 entries)
        if (user.SubscriptionTier == SubscriptionTier.Free)
        {
            var entryCount = await _context.VaultEntries
                .Where(v => v.UserId == request.UserId && !v.IsDeleted)
                .CountAsync(cancellationToken);

            if (entryCount >= 3)
            {
                throw new InvalidOperationException("Free tier limit reached. Upgrade to Premium for unlimited entries.");
            }
        }

        // Create vault entry
        var vaultEntry = new VaultEntry
        {
            UserId = request.UserId,
            Title = request.Title,
            Category = Enum.Parse<VaultCategory>(request.Category),
            EncryptedDataKey = request.EncryptedDataKey,
            EncryptedContent = request.EncryptedContent,
            IV = request.IV,
            IsSharedWithHeirs = request.IsSharedWithHeirs
        };

        _context.VaultEntries.Add(vaultEntry);
        await _context.SaveChangesAsync(cancellationToken);

        // If shared with heirs, create heir access entries
        if (request.IsSharedWithHeirs)
        {
            var heirs = await _context.Heirs
                .Where(h => h.UserId == request.UserId && h.IsVerified && !h.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var heir in heirs)
            {
                // In production, would encrypt DEK with heir's public key here
                var heirAccess = new HeirVaultAccess
                {
                    HeirId = heir.Id,
                    VaultEntryId = vaultEntry.Id,
                    EncryptedDataKey = request.EncryptedDataKey // Placeholder
                };

                _context.HeirVaultAccesses.Add(heirAccess);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        return new VaultEntryDto
        {
            Id = vaultEntry.Id,
            Title = vaultEntry.Title,
            Category = vaultEntry.Category.ToString(),
            EncryptedDataKey = vaultEntry.EncryptedDataKey,
            EncryptedContent = vaultEntry.EncryptedContent,
            IV = vaultEntry.IV,
            EncryptionAlgorithm = vaultEntry.EncryptionAlgorithm,
            IsSharedWithHeirs = vaultEntry.IsSharedWithHeirs,
            CreatedAt = vaultEntry.CreatedAt,
            UpdatedAt = vaultEntry.UpdatedAt
        };
    }
}
