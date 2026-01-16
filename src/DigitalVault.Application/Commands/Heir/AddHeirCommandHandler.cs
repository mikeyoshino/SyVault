using System.Security.Cryptography;
using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Enums;
using DigitalVault.Shared.DTOs.Heir;
using MediatR;

namespace DigitalVault.Application.Commands.Heir;

public class AddHeirCommandHandler : IRequestHandler<AddHeirCommand, HeirDto>
{
    private readonly IApplicationDbContext _context;

    public AddHeirCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HeirDto> Handle(AddHeirCommand request, CancellationToken cancellationToken)
    {
        // Get user to check subscription tier
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Check subscription tier limits
        if (user.SubscriptionTier == SubscriptionTier.Free)
        {
            var heirCount = await _context.Heirs
                .Where(h => h.UserId == request.UserId && !h.IsDeleted)
                .CountAsync(cancellationToken);

            if (heirCount >= 1)
            {
                throw new InvalidOperationException("Free tier limit reached. Maximum 1 heir allowed. Upgrade to Premium for unlimited heirs.");
            }
        }

        // Check if heir already exists for this user
        var existingHeir = await _context.Heirs
            .Where(h => h.UserId == request.UserId && h.Email == request.Email && !h.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingHeir != null)
        {
            throw new InvalidOperationException($"Heir with email {request.Email} already exists");
        }

        // Generate verification token
        var verificationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        // Parse AccessLevel enum
        if (!Enum.TryParse<AccessLevel>(request.AccessLevel, out var accessLevel))
        {
            accessLevel = AccessLevel.Full;
        }

        // Decode public key from base64
        byte[] publicKeyBytes;
        try
        {
            publicKeyBytes = Convert.FromBase64String(request.PublicKey);
        }
        catch
        {
            throw new InvalidOperationException("Invalid public key format. Must be base64 encoded.");
        }

        // Create heir
        var heir = new Domain.Entities.Heir
        {
            UserId = request.UserId,
            Email = request.Email,
            FullName = request.FullName,
            Relationship = request.Relationship,
            IsVerified = false,
            VerificationToken = verificationToken,
            VerificationExpiresAt = DateTime.UtcNow.AddDays(7), // Token expires in 7 days
            PublicKey = publicKeyBytes,
            AccessLevel = accessLevel,
            CanAccessCategories = request.CanAccessCategories ?? new List<string>(),
            User = user
        };

        _context.Heirs.Add(heir);
        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Send verification email to heir
        // await _emailService.SendHeirVerificationEmail(heir.Email, heir.FullName, verificationToken);

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
