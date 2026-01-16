using MediatR;

namespace DigitalVault.Application.Commands.Auth;

public class VerifyMfaCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
}
