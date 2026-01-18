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
                _logger.LogInformation("Generating JWT for user: {User}", context.User.Identity?.Name);

                var jwt = GenerateJwtFromClaims(context.User.Claims);

                if (string.IsNullOrEmpty(jwt))
                {
                    _logger.LogError("Generated JWT is EMPTY/NULL!");
                }
                else
                {
                    // _logger.LogInformation("Generated JWT length: {Length}", jwt.Length);
                    context.Request.Headers["Authorization"] = $"Bearer {jwt}";
                    _logger.LogInformation("Injected Authorization header for API request");
                }
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

        // Filter out claims that might cause issues or duplicates
        // Note: Cookie claims need to be mapped correctly to JWT claims
        var tokenClaims = claims.Where(c =>
            c.Type != "aud" &&
            c.Type != "iss" &&
            c.Type != "exp" &&
            c.Type != "nbf");

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: tokenClaims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
