using DigitalVault.Application.Interfaces;
using MediatR;

namespace DigitalVault.Application.Commands.Auth;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Unit>
{
    private readonly ITokenService _tokenService;

    public RevokeTokenCommandHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        if (request.RevokeAll)
        {
            // Revoke all tokens for the user
            await _tokenService.RevokeAllUserTokensAsync(request.UserId);
        }
        else if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            // Revoke specific token
            await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
        }

        return Unit.Value;
    }
}
