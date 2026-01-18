using DigitalVault.Logic.Services;
using DigitalVault.Shared.DTOs.FamilyMembers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalVault.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FamilyMembersController : ControllerBase
{
    private readonly FamilyMemberService _familyMemberService;
    private readonly ILogger<FamilyMembersController> _logger;

    public FamilyMembersController(FamilyMemberService familyMemberService, ILogger<FamilyMembersController> logger)
    {
        _familyMemberService = familyMemberService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FamilyMemberDto>>> GetFamilyMembers()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting family members for User: {UserId}", userId);

            // Fetch members using UserID (creates Account if missing)
            var members = await _familyMemberService.GetFamilyMembersByUserIdAsync(userId);

            _logger.LogInformation("Found {Count} members", members.Count());

            var dtos = members.Select(m => new FamilyMemberDto
            {
                Id = m.Id,
                EncryptedFirstName = m.EncryptedFirstName,
                EncryptedLastName = m.EncryptedLastName,
                EncryptedRelationship = m.EncryptedRelationship,
                AvatarColor = m.AvatarColor,
                InitialsPlainText = m.InitialsPlainText
            });

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting family members");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FamilyMemberDto>> GetFamilyMember(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var member = await _familyMemberService.GetFamilyMemberByUserIdAsync(id, userId);

            if (member == null)
            {
                return NotFound();
            }

            return Ok(new FamilyMemberDto
            {
                Id = member.Id,
                EncryptedFirstName = member.EncryptedFirstName,
                EncryptedLastName = member.EncryptedLastName,
                EncryptedRelationship = member.EncryptedRelationship,
                AvatarColor = member.AvatarColor,
                InitialsPlainText = member.InitialsPlainText
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting family member details");
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        // 1. Try standard claims (reliable)
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value
                          ?? User.FindFirst("userId")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("User ID not found in token");
    }
}
