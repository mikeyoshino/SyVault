using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Entities;
using DigitalVault.Shared.DTOs.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DigitalVault.Application.Commands.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Hash password
        var (passwordHash, salt) = _passwordHasher.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = passwordHash,
            PasswordSalt = salt,
            IsActive = true,
            LastLoginAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        // Create default account
        var account = new Account
        {
            UserId = user.Id,
            EncryptedAccountName = "My Vault", // Placeholder
            IsDefault = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            // Initialize with empty encryption fields for now
            EncryptedMasterKey = "",
            MasterKeySalt = "",
            AuthenticationTag = ""
        };
        _context.Accounts.Add(account);

        await _context.SaveChangesAsync(cancellationToken);

        // Generate tokens
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
