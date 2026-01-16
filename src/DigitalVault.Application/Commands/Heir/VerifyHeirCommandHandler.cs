using DigitalVault.Application.Interfaces;
using DigitalVault.Shared.DTOs.Heir;
using MediatR;

namespace DigitalVault.Application.Commands.Heir;

public class VerifyHeirCommandHandler : IRequestHandler<VerifyHeirCommand, HeirDto>
{
    private readonly IApplicationDbContext _context;

    public VerifyHeirCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HeirDto> Handle(VerifyHeirCommand request, CancellationToken cancellationToken)
    {
        var heir = await _context.Heirs
            .Where(h => h.VerificationToken == request.VerificationToken && !h.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (heir == null)
        {
            throw new InvalidOperationException("Invalid verification token");
        }

        if (heir.IsVerified)
        {
            throw new InvalidOperationException("Heir already verified");
        }

        if (heir.VerificationExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Verification token has expired");
        }

        heir.IsVerified = true;
        heir.VerifiedAt = DateTime.UtcNow;
        heir.VerificationToken = null; // Clear token after verification

        await _context.SaveChangesAsync(cancellationToken);

        return new HeirDto
        {
            Id = heir.Id,
            Email = heir.Email,
            FullName = heir.FullName,
            Relationship = heir.Relationship,
            IsVerified = heir.IsVerified,
            VerifiedAt = heir.VerifiedAt,
            AccessLevel = heir.AccessLevel.ToString(),
            CanAccessCategories = heir.CanAccessCategories,
            CreatedAt = heir.CreatedAt,
            UpdatedAt = heir.UpdatedAt
        };
    }
}
