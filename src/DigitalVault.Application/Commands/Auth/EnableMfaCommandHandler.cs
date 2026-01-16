using DigitalVault.Application.Interfaces;
using DigitalVault.Shared.DTOs.Auth;
using MediatR;

namespace DigitalVault.Application.Commands.Auth;

public class EnableMfaCommandHandler : IRequestHandler<EnableMfaCommand, EnableMfaResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IMfaService _mfaService;

    public EnableMfaCommandHandler(
        IApplicationDbContext context,
        IMfaService mfaService)
    {
        _context = context;
        _mfaService = mfaService;
    }

    public async Task<EnableMfaResponse> Handle(EnableMfaCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        if (user.MfaEnabled)
        {
            throw new InvalidOperationException("MFA is already enabled. Disable it first to set up a new secret.");
        }

        // Generate new MFA secret
        var secret = _mfaService.GenerateSecret();
        var qrCodeUrl = _mfaService.GenerateQrCodeUrl(secret, user.Email);

        // Store the secret but don't enable MFA yet (user needs to verify first)
        user.MfaSecret = secret;
        await _context.SaveChangesAsync(cancellationToken);

        return new EnableMfaResponse
        {
            Secret = secret,
            QrCodeUrl = qrCodeUrl,
            ManualEntryKey = secret.ToLower().Insert(4, " ").Insert(9, " ").Insert(14, " ").Insert(19, " "), // Format: xxxx xxxx xxxx xxxx xxxx
            Issuer = "DigitalVault",
            AccountName = user.Email
        };
    }
}
