using DigitalVault.Application.Interfaces;
using DigitalVault.Shared.DTOs.Auth;
using MediatR;

namespace DigitalVault.Application.Commands.Auth;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Validate old refresh token
        var oldToken = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken);
        if (oldToken == null)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        // Rotate: generate new tokens, revoke old
        var (newAccessToken, newRefreshToken) = await _tokenService.RotateRefreshTokenAsync(oldToken);

        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = oldToken.User.Id,
                Email = oldToken.User.Email,
                EmailVerified = oldToken.User.EmailVerified,
                PhoneNumber = oldToken.User.PhoneNumber,
                MfaEnabled = oldToken.User.MfaEnabled,
                SubscriptionTier = oldToken.User.SubscriptionTier.ToString(),
                SubscriptionExpiresAt = oldToken.User.SubscriptionExpiresAt,
                KeyDerivationSalt = oldToken.User.KeyDerivationSalt,
                KeyDerivationIterations = oldToken.User.KeyDerivationIterations,
                EncryptedMasterKey = oldToken.User.EncryptedMasterKey
            }
        };
    }
}
