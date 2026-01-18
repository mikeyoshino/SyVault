using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using DigitalVault.Infrastructure.Data;
using Microsoft.Extensions.Options;
using DigitalVault.Infrastructure.Configuration;
using DigitalVault.Logic.Services;
using DigitalVault.Shared.DTOs.Documents;

namespace DigitalVault.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentService _documentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DocumentsController> _logger;
    private readonly AwsSettings _awsSettings;

    public DocumentsController(
        DocumentService documentService,
        IUnitOfWork unitOfWork,
        ILogger<DocumentsController> logger,
        IOptions<AwsSettings> awsSettings)
    {
        _documentService = documentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _awsSettings = awsSettings.Value;
    }

    [HttpPost("presigned-url/upload")]
    public async Task<ActionResult<object>> GetUploadUrl([FromBody] UploadRequest request)
    {
        // For upload URL, we might just need UserId if using S3 paths like /userId/...
        // But let's act robustly.
        var userId = GetCurrentUserId();
        var (uploadUrl, objectKey) = await _documentService.GenerateUploadUrlAsync(request.ContentType, userId);

        return Ok(new { UploadUrl = uploadUrl, ObjectKey = objectKey });
    }

    [HttpPost]
    public async Task<ActionResult<Document>> CreateDocument([FromBody] DigitalVault.Shared.DTOs.Documents.DocumentDto request)
    {
        var accountId = await GetAccountIdForUserAsync();

        var document = new Document
        {
            FamilyMemberId = request.FamilyMemberId,
            AccountId = accountId,

            DocumentType = request.DocumentType,
            S3BucketName = _awsSettings.BucketName,
            S3ObjectKey = request.S3ObjectKey,
            S3Region = _awsSettings.Region,

            EncryptedOriginalFileName = request.EncryptedOriginalFileName,
            EncryptedFileExtension = request.EncryptedFileExtension,
            EncryptedFileSize = request.EncryptedFileSize,
            EncryptedMimeType = request.EncryptedMimeType,

            EncryptionIV = request.EncryptionIV,
            EncryptionTag = request.EncryptionTag,

            UploadedAt = DateTime.UtcNow
        };

        await _documentService.CreateDocumentAsync(document);

        // Return DTO instead of entity to avoid circular reference
        var dto = new DocumentDto
        {
            Id = document.Id,
            FamilyMemberId = document.FamilyMemberId,
            DocumentType = document.DocumentType,
            S3ObjectKey = document.S3ObjectKey,
            EncryptedOriginalFileName = document.EncryptedOriginalFileName,
            EncryptedFileExtension = document.EncryptedFileExtension,
            EncryptedFileSize = document.EncryptedFileSize,
            EncryptedMimeType = document.EncryptedMimeType,
            EncryptionIV = document.EncryptionIV,
            EncryptionTag = document.EncryptionTag,
            UploadedAt = document.UploadedAt
        };

        return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, dto);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Document>> GetDocument(Guid id)
    {
        var document = await _documentService.GetDocumentAsync(id);
        if (document == null) return NotFound();

        // Security check: Ensure document belongs to user's account
        var accountId = await GetAccountIdForUserAsync();
        if (document.AccountId != accountId)
        {
            return Forbid();
        }

        return document;
    }

    [HttpGet("family/{familyMemberId}")]
    public async Task<ActionResult<IEnumerable<DigitalVault.Shared.DTOs.Documents.DocumentDto>>> GetDocumentsByFamilyMember(Guid familyMemberId)
    {
        var accountId = await GetAccountIdForUserAsync();
        var documents = await _documentService.GetDocumentsByFamilyMemberAsync(familyMemberId, accountId);

        // Map Entities to DTOs
        var dtos = documents.Select(d => new DigitalVault.Shared.DTOs.Documents.DocumentDto
        {
            Id = d.Id,
            FamilyMemberId = d.FamilyMemberId,
            DocumentType = d.DocumentType,
            S3ObjectKey = d.S3ObjectKey,
            UploadedAt = d.UploadedAt,
            EncryptedOriginalFileName = d.EncryptedOriginalFileName,
            EncryptedFileExtension = d.EncryptedFileExtension,
            EncryptedFileSize = d.EncryptedFileSize,
            EncryptedMimeType = d.EncryptedMimeType,
            EncryptionIV = d.EncryptionIV,
            EncryptionTag = d.EncryptionTag
        });

        return Ok(dtos);
    }

    [HttpGet("{id}/preview-url")]
    public async Task<ActionResult<object>> GetPreviewUrl(Guid id)
    {
        try
        {
            var accountId = await GetAccountIdForUserAsync();
            var url = await _documentService.GeneratePreviewUrlAsync(id, accountId);
            return Ok(new { Url = url });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        try
        {
            var accountId = await GetAccountIdForUserAsync();
            await _documentService.DeleteDocumentAsync(id, accountId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value
                          ?? User.FindFirst("userId")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("User ID not found in token");
    }

    private async Task<Guid> GetAccountIdForUserAsync()
    {
        var userId = GetCurrentUserId();

        // Look up account via UnitOfWork
        var accounts = await _unitOfWork.Accounts.FindAsync(a => a.UserId == userId);
        var account = accounts.OrderByDescending(a => a.IsDefault).FirstOrDefault();

        if (account != null)
        {
            return account.Id;
        }

        // If no account exists, we could Create one here or throw.
        // Since FamilyMembers controller creates it on load, it SHOULD exist.
        // But for safety, let's create it if missing (Auto-heal).

        _logger.LogInformation("No account found for user {UserId} in Documents controller. Creating one.", userId);

        var newAccount = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EncryptedAccountName = "My Vault",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EncryptedMasterKey = "",
            MasterKeySalt = "",
            AuthenticationTag = ""
        };

        await _unitOfWork.Accounts.AddAsync(newAccount);
        await _unitOfWork.CompleteAsync();

        return newAccount.Id;
    }
}

public class UploadRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
