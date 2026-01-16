using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Enums;
using DigitalVault.Shared.DTOs.Heir;
using MediatR;

namespace DigitalVault.Application.Commands.Heir;

public class UpdateHeirCommandHandler : IRequestHandler<UpdateHeirCommand, HeirDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateHeirCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HeirDto> Handle(UpdateHeirCommand request, CancellationToken cancellationToken)
    {
        var heir = await _context.Heirs
            .Where(h => h.Id == request.Id && h.UserId == request.UserId && !h.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (heir == null)
        {
            throw new InvalidOperationException("Heir not found");
        }

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            heir.FullName = request.FullName;
        }

        if (request.Relationship != null)
        {
            heir.Relationship = request.Relationship;
        }

        if (!string.IsNullOrWhiteSpace(request.AccessLevel))
        {
            if (Enum.TryParse<AccessLevel>(request.AccessLevel, out var accessLevel))
            {
                heir.AccessLevel = accessLevel;
            }
        }

        if (request.CanAccessCategories != null)
        {
            heir.CanAccessCategories = request.CanAccessCategories;
        }

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
