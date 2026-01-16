using DigitalVault.Application.Commands.Vault;
using DigitalVault.Application.Queries.Vault;
using DigitalVault.Shared.DTOs.Common;
using DigitalVault.Shared.DTOs.Vault;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalVault.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VaultController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<VaultController> _logger;

    public VaultController(IMediator mediator, ILogger<VaultController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all vault entries for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<VaultEntryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<VaultEntryDto>>>> GetVaultEntries(
        [FromQuery] string? category = null)
    {
        try
        {
            var userId = GetCurrentUserId();

            var query = new GetVaultEntriesQuery
            {
                UserId = userId,
                Category = category
            };

            var result = await _mediator.Send(query);

            return Ok(ApiResponse<List<VaultEntryDto>>.SuccessResponse(
                result,
                $"Retrieved {result.Count} vault entries"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vault entries");
            return StatusCode(500, ApiResponse<List<VaultEntryDto>>.ErrorResponse(
                "An error occurred while retrieving vault entries"
            ));
        }
    }

    /// <summary>
    /// Get a specific vault entry by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<VaultEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VaultEntryDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VaultEntryDto>>> GetVaultEntry(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();

            var query = new GetVaultEntryQuery
            {
                Id = id,
                UserId = userId
            };

            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(ApiResponse<VaultEntryDto>.ErrorResponse("Vault entry not found"));
            }

            return Ok(ApiResponse<VaultEntryDto>.SuccessResponse(
                result,
                "Vault entry retrieved successfully"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vault entry {VaultEntryId}", id);
            return StatusCode(500, ApiResponse<VaultEntryDto>.ErrorResponse(
                "An error occurred while retrieving the vault entry"
            ));
        }
    }

    /// <summary>
    /// Create a new vault entry
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VaultEntryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<VaultEntryDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<VaultEntryDto>>> CreateVaultEntry(
        [FromBody] CreateVaultEntryRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();

            var command = new CreateVaultEntryCommand
            {
                UserId = userId,
                Title = request.Title,
                Category = request.Category,
                EncryptedDataKey = request.EncryptedDataKey,
                EncryptedContent = request.EncryptedContent,
                IV = request.IV,
                IsSharedWithHeirs = request.IsSharedWithHeirs
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation(
                "Vault entry created: {VaultEntryId} for user {UserId}",
                result.Id,
                userId
            );

            return CreatedAtAction(
                nameof(GetVaultEntry),
                new { id = result.Id },
                ApiResponse<VaultEntryDto>.SuccessResponse(
                    result,
                    "Vault entry created successfully"
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create vault entry: {Message}", ex.Message);
            return BadRequest(ApiResponse<VaultEntryDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vault entry");
            return StatusCode(500, ApiResponse<VaultEntryDto>.ErrorResponse(
                "An error occurred while creating the vault entry"
            ));
        }
    }

    /// <summary>
    /// Update an existing vault entry
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<VaultEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VaultEntryDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VaultEntryDto>>> UpdateVaultEntry(
        Guid id,
        [FromBody] UpdateVaultEntryRequest request)
    {
        // TODO: Implement UpdateVaultEntryCommand
        return Ok(ApiResponse<VaultEntryDto>.SuccessResponse(
            null!,
            "Update not implemented yet"
        ));
    }

    /// <summary>
    /// Delete a vault entry (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVaultEntry(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();

            var command = new DeleteVaultEntryCommand
            {
                Id = id,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Vault entry not found"));
            }

            _logger.LogInformation(
                "Vault entry deleted: {VaultEntryId} for user {UserId}",
                id,
                userId
            );

            return Ok(ApiResponse<object>.SuccessResponse(
                new { id },
                "Vault entry deleted successfully"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vault entry {VaultEntryId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while deleting the vault entry"
            ));
        }
    }

    /// <summary>
    /// Get vault statistics for current user
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetStatistics()
    {
        try
        {
            var userId = GetCurrentUserId();

            var query = new GetVaultEntriesQuery
            {
                UserId = userId
            };

            var entries = await _mediator.Send(query);

            var statistics = new
            {
                totalEntries = entries.Count,
                categoryCounts = entries.GroupBy(e => e.Category)
                    .Select(g => new { category = g.Key, count = g.Count() })
                    .ToList(),
                sharedWithHeirs = entries.Count(e => e.IsSharedWithHeirs),
                lastCreated = entries.Any() ? entries.Max(e => e.CreatedAt) : (DateTime?)null
            };

            return Ok(ApiResponse<object>.SuccessResponse(
                statistics,
                "Statistics retrieved successfully"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vault statistics");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "An error occurred while retrieving statistics"
            ));
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
}
