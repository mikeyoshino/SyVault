using DigitalVault.Application.Commands.Auth;
using DigitalVault.Application.Interfaces;
using DigitalVault.Shared.DTOs.Auth;
using DigitalVault.Shared.DTOs.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalVault.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    private readonly ITokenService _tokenService;

    public AuthController(IMediator mediator, ILogger<AuthController> logger, ITokenService tokenService)
    {
        _mediator = mediator;
        _logger = logger;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterCommand
            {
                Email = request.Email,
                Password = request.Password,
                PhoneNumber = request.PhoneNumber,
                KeyDerivationSalt = request.KeyDerivationSalt,
                KeyDerivationIterations = request.KeyDerivationIterations,
                EncryptedMasterKey = request.EncryptedMasterKey,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                UserAgent = Request.Headers["User-Agent"].ToString()
            };

            var result = await _mediator.Send(command);

            // Set httpOnly cookies (tokens no longer in response body)
            SetAuthCookies(
                result.AccessToken,
                result.RefreshToken,
                result.ExpiresAt,
                DateTime.UtcNow.AddDays(7)
            );

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            // Return only user info (no tokens)
            return Ok(ApiResponse<UserDto>.SuccessResponse(
                result.User,
                "Registration successful"
            ));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("An error occurred during registration"));
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password,
                MfaCode = request.MfaCode,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                UserAgent = Request.Headers["User-Agent"].ToString()
            };

            var result = await _mediator.Send(command);

            // Set httpOnly cookies (tokens no longer in response body)
            SetAuthCookies(
                result.AccessToken,
                result.RefreshToken,
                result.ExpiresAt,
                DateTime.UtcNow.AddDays(7)
            );

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            // Return only user info (no tokens)
            return Ok(ApiResponse<UserDto>.SuccessResponse(
                result.User,
                "Login successful"
            ));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for {Email}: {Message}", request.Email, ex.Message);
            return Unauthorized(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("An error occurred during login"));
        }
    }

    /// <summary>
    /// Enable MFA (Step 1: Get QR code)
    /// </summary>
    [HttpPost("mfa/enable")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EnableMfaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnableMfaResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<EnableMfaResponse>>> EnableMfa()
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new EnableMfaCommand { UserId = userId };
            var result = await _mediator.Send(command);

            _logger.LogInformation("MFA setup initiated for user: {UserId}", userId);

            return Ok(ApiResponse<EnableMfaResponse>.SuccessResponse(
                result,
                "MFA setup initiated. Scan QR code with authenticator app."
            ));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("MFA enable failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<EnableMfaResponse>.ErrorResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("MFA enable unauthorized: {Message}", ex.Message);
            return Unauthorized(ApiResponse<EnableMfaResponse>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MFA enable");
            return StatusCode(500, ApiResponse<EnableMfaResponse>.ErrorResponse("An error occurred while enabling MFA"));
        }
    }

    /// <summary>
    /// Verify MFA code (Step 2: Activate MFA)
    /// </summary>
    [HttpPost("mfa/verify")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> VerifyMfa([FromBody] VerifyMfaRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new VerifyMfaCommand { UserId = userId, Code = request.Code };
            var result = await _mediator.Send(command);

            if (!result)
            {
                _logger.LogWarning("Invalid MFA code provided for user: {UserId}", userId);
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid MFA code"));
            }

            _logger.LogInformation("MFA enabled successfully for user: {UserId}", userId);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { },
                "MFA enabled successfully"
            ));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("MFA verify failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("MFA verify unauthorized: {Message}", ex.Message);
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MFA verification");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while verifying MFA"));
        }
    }

    /// <summary>
    /// Disable MFA
    /// </summary>
    [HttpPost("mfa/disable")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> DisableMfa([FromBody] VerifyMfaRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new DisableMfaCommand { UserId = userId, Code = request.Code };
            var result = await _mediator.Send(command);

            if (!result)
            {
                _logger.LogWarning("Invalid MFA code provided for user: {UserId}", userId);
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid MFA code"));
            }

            _logger.LogInformation("MFA disabled successfully for user: {UserId}", userId);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { },
                "MFA disabled successfully"
            ));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("MFA disable failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("MFA disable unauthorized: {Message}", ex.Message);
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MFA disable");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while disabling MFA"));
        }
    }

    /// <summary>
    /// Refresh access token using refresh token from cookie
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserDto>>> RefreshToken()
    {
        try
        {
            // Read refresh token from cookie
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh token missing from cookie");
                return Unauthorized(ApiResponse<UserDto>.ErrorResponse("No refresh token provided"));
            }

            var command = new RefreshTokenCommand
            {
                RefreshToken = refreshToken,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                UserAgent = Request.Headers["User-Agent"].ToString()
            };

            var result = await _mediator.Send(command);

            // Set new cookies with rotated tokens
            SetAuthCookies(
                result.AccessToken,
                result.RefreshToken,
                result.ExpiresAt,
                DateTime.UtcNow.AddDays(7)
            );

            _logger.LogInformation("Token refreshed successfully for user: {UserId}", result.User.Id);

            // Return only user info (no tokens)
            return Ok(ApiResponse<UserDto>.SuccessResponse(
                result.User,
                "Token refreshed successfully"
            ));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            ClearAuthCookies();  // Clear invalid cookies
            return Unauthorized(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("An error occurred during token refresh"));
        }
    }

    /// <summary>
    /// Logout (invalidate current refresh token and clear cookies)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        try
        {
            var userId = GetCurrentUserId();

            // Revoke refresh token if present in cookie
            if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken) && !string.IsNullOrEmpty(refreshToken))
            {
                var command = new RevokeTokenCommand
                {
                    RefreshToken = refreshToken,
                    UserId = userId
                };
                await _mediator.Send(command);
            }

            // Clear cookies
            ClearAuthCookies();

            _logger.LogInformation("User logged out: {UserId}", userId);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { },
                "Logout successful"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            // Clear cookies anyway
            ClearAuthCookies();
            return Ok(ApiResponse<object>.SuccessResponse(
                new { },
                "Logout successful"
            ));
        }
    }

    /// <summary>
    /// Logout from all devices (revoke all refresh tokens)
    /// </summary>
    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> LogoutAll()
    {
        try
        {
            var userId = GetCurrentUserId();

            var command = new RevokeTokenCommand
            {
                RevokeAll = true,
                UserId = userId
            };
            await _mediator.Send(command);

            // Clear cookies on current device
            ClearAuthCookies();

            _logger.LogInformation("User logged out from all devices: {UserId}", userId);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { },
                "Logged out from all devices successfully"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout all");
            ClearAuthCookies();
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during logout"));
        }
    }

    /// <summary>
    /// Get all active sessions/devices for the current user
    /// </summary>
    [HttpGet("sessions")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<SessionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<SessionDto>>>> GetActiveSessions()
    {
        try
        {
            var userId = GetCurrentUserId();
            var sessions = await _tokenService.GetUserActiveSessionsAsync(userId);

            var sessionDtos = sessions.Select(s => new SessionDto
            {
                Id = s.Id,
                DeviceName = s.DeviceName,
                IpAddress = s.IpAddress,
                LastUsedAt = s.LastUsedAt,
                CreatedAt = s.CreatedAt
            }).ToList();

            _logger.LogInformation("Retrieved {Count} active sessions for user: {UserId}", sessionDtos.Count, userId);

            return Ok(ApiResponse<List<SessionDto>>.SuccessResponse(
                sessionDtos,
                "Active sessions retrieved successfully"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active sessions");
            return StatusCode(500, ApiResponse<List<SessionDto>>.ErrorResponse("An error occurred while retrieving sessions"));
        }
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        // TODO: Implement get current user
        return Ok(ApiResponse<UserDto>.SuccessResponse(
            new UserDto { },
            "User information retrieved"
        ));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }

    private void SetAuthCookies(string accessToken, string refreshToken, DateTime accessExpiry, DateTime refreshExpiry)
    {
        // Access token cookie - sent with all API requests
        Response.Cookies.Append("accessToken", accessToken, new CookieOptions
        {
            HttpOnly = true,  // JavaScript cannot access (XSS protection)
            Secure = true,    // HTTPS only
            SameSite = SameSiteMode.Strict,  // CSRF protection
            Expires = accessExpiry,
            Path = "/"
        });

        // Refresh token cookie - only sent to auth endpoints
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = refreshExpiry,
            Path = "/api/auth"  // Restrict to auth endpoints only
        });
    }

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete("accessToken", new CookieOptions { Path = "/" });
        Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/api/auth" });
    }
}
