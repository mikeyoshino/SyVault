using DigitalVault.Application.Commands.Auth;
using DigitalVault.Shared.DTOs.Auth;
using DigitalVault.Shared.DTOs.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalVault.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
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
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                UserAgent = Request.Headers["User-Agent"].ToString()
            };

            var result = await _mediator.Send(command);

            // Set httpOnly cookies
            SetAuthCookies(
                result.AccessToken,
                result.RefreshToken,
                result.ExpiresAt,
                DateTime.UtcNow.AddDays(7)
            );

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

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
    [AllowAnonymous]
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

            // Set httpOnly cookies
            SetAuthCookies(
                result.AccessToken,
                result.RefreshToken,
                result.ExpiresAt,
                DateTime.UtcNow.AddDays(7)
            );

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

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
    /// Logout (clear cookies)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> Logout()
    {
        try
        {
            ClearAuthCookies();

            _logger.LogInformation("User logged out");

            return Ok(ApiResponse<object>.SuccessResponse(
                new { },
                "Logout successful"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            ClearAuthCookies();
            return Ok(ApiResponse<object>.SuccessResponse(
                new { },
                "Logout successful"
            ));
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
        try
        {
            var userId = GetCurrentUserId();

            // TODO: Implement get current user from database
            var userDto = new UserDto
            {
                Id = userId,
                Email = "user@example.com"
            };

            return Ok(ApiResponse<UserDto>.SuccessResponse(
                userDto,
                "User information retrieved"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("An error occurred"));
        }
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
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps, // Dynamic secure flag
            SameSite = SameSiteMode.Lax, // Lax is better for navigation
            Expires = accessExpiry,
            Path = "/"
        };

        Response.Cookies.Append("accessToken", accessToken, cookieOptions);

        cookieOptions.Expires = refreshExpiry;
        cookieOptions.Path = "/api/auth";
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete("accessToken", new CookieOptions { Path = "/" });
        Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/api/auth" });
    }
}
