using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Entities;
using DigitalVault.Shared.DTOs.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DigitalVault.Application.Commands.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is disabled");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // Fetch User's Account
        var account = await _context.Accounts
            .Where(a => a.UserId == user.Id)
            .OrderByDescending(a => a.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);

        if (account == null)
        {
            _logger.LogInformation("No account found for user {UserId}. Creating default account.", user.Id);
            account = new Account
            {
                UserId = user.Id,
                EncryptedAccountName = "My Vault",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EncryptedMasterKey = "",
                MasterKeySalt = "",
                AuthenticationTag = ""
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            _logger.LogInformation("Found existing account {AccountId} for user {UserId}", account.Id, user.Id);
        }

        // Generate tokens
        _logger.LogInformation("Generating token for user {UserId} with AccountId {AccountId}", user.Id, account.Id);
        var accessToken = _tokenService.GenerateAccessToken(user, account.Id);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token
        await _tokenService.StoreRefreshTokenAsync(
            user.Id,
            refreshToken,
            "Web Browser",
            request.IpAddress,
            request.UserAgent,
            cancellationToken);

        // Map to DTO
        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return new AuthResponse
        {
            User = userDto,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }
}
