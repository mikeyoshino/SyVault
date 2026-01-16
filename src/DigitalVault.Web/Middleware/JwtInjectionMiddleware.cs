using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DigitalVault.Web.Middleware;

public class JwtInjectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtInjectionMiddleware> _logger;

    public JwtInjectionMiddleware(
        RequestDelegate next, 
        IConfiguration configuration,
        ILogger<JwtInjectionMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only process API requests
        if (context.Request.Path.StartsWithSegments("/api") &&
            context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                var jwt = GenerateJwtFromClaims(context.User.Claims);
                context.Request.Headers["Authorization"] = $"Bearer {jwt}";
                
                _logger.LogDebug("JWT token injected for API request: {Path}", context.Request.Path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT for API request");
                // Don't block the request, let it proceed without JWT
            }
        }

        await _next(context);
    }

    private string GenerateJwtFromClaims(IEnumerable<Claim> claims)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] 
            ?? throw new InvalidOperationException("JWT SecretKey is not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
