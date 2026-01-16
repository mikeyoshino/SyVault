using DigitalVault.Application.Interfaces;
using MediatR;

namespace DigitalVault.Application.Commands.Auth;

public class DisableMfaCommandHandler : IRequestHandler<DisableMfaCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IMfaService _mfaService;

    public DisableMfaCommandHandler(
        IApplicationDbContext context,
        IMfaService mfaService)
    {
        _context = context;
        _mfaService = mfaService;
    }

    public async Task<bool> Handle(DisableMfaCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        if (!user.MfaEnabled)
        {
            throw new InvalidOperationException("MFA is not enabled");
        }

        // Verify the code before disabling (for security)
        var isValid = _mfaService.VerifyCode(user.MfaSecret!, request.Code);

        if (!isValid)
        {
            return false;
        }

        // Disable MFA and clear the secret
        user.MfaEnabled = false;
        user.MfaSecret = null;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
