using DigitalVault.Application.Interfaces;
using MediatR;

namespace DigitalVault.Application.Commands.Auth;

public class VerifyMfaCommandHandler : IRequestHandler<VerifyMfaCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IMfaService _mfaService;

    public VerifyMfaCommandHandler(
        IApplicationDbContext context,
        IMfaService mfaService)
    {
        _context = context;
        _mfaService = mfaService;
    }

    public async Task<bool> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        if (string.IsNullOrEmpty(user.MfaSecret))
        {
            throw new InvalidOperationException("MFA secret not found. Please enable MFA first.");
        }

        // Verify the code
        var isValid = _mfaService.VerifyCode(user.MfaSecret, request.Code);

        if (!isValid)
        {
            return false;
        }

        // Activate MFA
        user.MfaEnabled = true;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
