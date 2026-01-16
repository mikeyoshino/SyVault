using DigitalVault.Application.Commands.Heir;
using DigitalVault.Application.Queries.Heir;
using DigitalVault.Shared.DTOs.Common;
using DigitalVault.Shared.DTOs.Heir;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalVault.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HeirController : ControllerBase
{
    private readonly IMediator _mediator;

    public HeirController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all heirs for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<HeirDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HeirDto>>>> GetHeirs([FromQuery] bool? isVerified = null)
    {
        var userId = GetCurrentUserId();
        var query = new GetHeirsQuery { UserId = userId, IsVerified = isVerified };
        var result = await _mediator.Send(query);

        return Ok(ApiResponse<List<HeirDto>>.SuccessResponse(
            result,
            $"Retrieved {result.Count} heir(s)"
        ));
    }

    /// <summary>
    /// Get a specific heir by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<HeirDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HeirDto>>> GetHeir(Guid id)
    {
        var userId = GetCurrentUserId();
        var query = new GetHeirQuery { Id = id, UserId = userId };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Heir not found"));
        }

        return Ok(ApiResponse<HeirDto>.SuccessResponse(result, "Heir retrieved successfully"));
    }

    /// <summary>
    /// Add a new heir
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HeirDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HeirDto>>> AddHeir([FromBody] AddHeirRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new AddHeirCommand
        {
            UserId = userId,
            Email = request.Email,
            FullName = request.FullName,
            Relationship = request.Relationship,
            PublicKey = request.PublicKey,
            AccessLevel = request.AccessLevel,
            CanAccessCategories = request.CanAccessCategories
        };

        var result = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetHeir),
            new { id = result.Id },
            ApiResponse<HeirDto>.SuccessResponse(
                result,
                "Heir added successfully. Verification email sent."
            )
        );
    }

    /// <summary>
    /// Verify heir with verification token (public endpoint)
    /// </summary>
    [HttpPost("verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<HeirDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HeirDto>>> VerifyHeir([FromQuery] string token)
    {
        var command = new VerifyHeirCommand { VerificationToken = token };
        var result = await _mediator.Send(command);

        return Ok(ApiResponse<HeirDto>.SuccessResponse(
            result,
            "Heir verified successfully"
        ));
    }

    /// <summary>
    /// Update heir information
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<HeirDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HeirDto>>> UpdateHeir(
        Guid id,
        [FromBody] UpdateHeirRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateHeirCommand
        {
            Id = id,
            UserId = userId,
            FullName = request.FullName,
            Relationship = request.Relationship,
            AccessLevel = request.AccessLevel,
            CanAccessCategories = request.CanAccessCategories
        };

        var result = await _mediator.Send(command);

        return Ok(ApiResponse<HeirDto>.SuccessResponse(
            result,
            "Heir updated successfully"
        ));
    }

    /// <summary>
    /// Remove (soft delete) an heir
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> RemoveHeir(Guid id)
    {
        var userId = GetCurrentUserId();
        var command = new RemoveHeirCommand { Id = id, UserId = userId };
        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Heir not found"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(
            new { id },
            "Heir removed successfully"
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
}
