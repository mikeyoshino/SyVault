using DigitalVault.Application.Commands.DeadManSwitch;
using DigitalVault.Application.Queries.DeadManSwitch;
using DigitalVault.Shared.DTOs.Common;
using DigitalVault.Shared.DTOs.DeadManSwitch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalVault.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeadManSwitchController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeadManSwitchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get Dead Man's Switch status
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DeadManSwitchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DeadManSwitchDto>>> GetSwitch()
    {
        var userId = GetCurrentUserId();
        var query = new GetSwitchQuery { UserId = userId };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Dead Man's Switch not found. Please setup the switch first."));
        }

        return Ok(ApiResponse<DeadManSwitchDto>.SuccessResponse(
            result,
            "Dead Man's Switch retrieved successfully"
        ));
    }

    /// <summary>
    /// Setup Dead Man's Switch for the first time
    /// </summary>
    [HttpPost("setup")]
    [ProducesResponseType(typeof(ApiResponse<DeadManSwitchDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<DeadManSwitchDto>>> SetupSwitch([FromBody] SetupSwitchRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new SetupSwitchCommand
        {
            UserId = userId,
            CheckInIntervalDays = request.CheckInIntervalDays,
            GracePeriodDays = request.GracePeriodDays,
            ReminderDays = request.ReminderDays,
            NotificationChannels = request.NotificationChannels,
            EmergencyEmail = request.EmergencyEmail,
            EmergencyPhone = request.EmergencyPhone
        };

        var result = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetSwitch),
            ApiResponse<DeadManSwitchDto>.SuccessResponse(
                result,
                "Dead Man's Switch setup successfully"
            )
        );
    }

    /// <summary>
    /// Update Dead Man's Switch settings
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<DeadManSwitchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DeadManSwitchDto>>> UpdateSwitch([FromBody] UpdateSwitchRequest request)
    {
        var userId = GetCurrentUserId();
        var command = new UpdateSwitchCommand
        {
            UserId = userId,
            CheckInIntervalDays = request.CheckInIntervalDays,
            GracePeriodDays = request.GracePeriodDays,
            ReminderDays = request.ReminderDays,
            NotificationChannels = request.NotificationChannels,
            EmergencyEmail = request.EmergencyEmail,
            EmergencyPhone = request.EmergencyPhone
        };

        var result = await _mediator.Send(command);

        return Ok(ApiResponse<DeadManSwitchDto>.SuccessResponse(
            result,
            "Dead Man's Switch updated successfully"
        ));
    }

    /// <summary>
    /// Check in to reset the countdown
    /// </summary>
    [HttpPost("checkin")]
    [ProducesResponseType(typeof(ApiResponse<CheckInResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CheckInResponse>>> CheckIn()
    {
        var userId = GetCurrentUserId();
        var command = new CheckInCommand { UserId = userId };
        var result = await _mediator.Send(command);

        return Ok(ApiResponse<CheckInResponse>.SuccessResponse(
            result,
            result.Message
        ));
    }

    /// <summary>
    /// Cancel (deactivate) Dead Man's Switch
    /// </summary>
    [HttpPost("cancel")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> CancelSwitch()
    {
        var userId = GetCurrentUserId();
        var command = new CancelSwitchCommand { UserId = userId };
        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Dead Man's Switch not found"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(
            new { },
            "Dead Man's Switch cancelled successfully"
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
