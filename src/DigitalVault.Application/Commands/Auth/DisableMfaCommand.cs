using MediatR;

namespace DigitalVault.Application.Commands.Auth;

public class DisableMfaCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty; // Require MFA code to disable for security
}
