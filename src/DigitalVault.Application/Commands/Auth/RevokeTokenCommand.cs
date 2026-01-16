using MediatR;

namespace DigitalVault.Application.Commands.Auth;

public class RevokeTokenCommand : IRequest<Unit>
{
    public string? RefreshToken { get; set; }  // Specific token
    public bool RevokeAll { get; set; }  // Or all user's tokens
    public Guid UserId { get; set; }
}
