using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using DigitalVault.Infrastructure.Data;

namespace DigitalVault.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IStorageService _storageService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IStorageService storageService,
        ApplicationDbContext context,
        ILogger<DocumentsController> logger)
    {
        _storageService = storageService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("presigned-url/upload")]
    public async Task<ActionResult<object>> GetUploadUrl([FromBody] UploadRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // Helper: Validate user owns the family member (omitted for brevity, should retain in production)

        var objectKey = $"accounts/{userId}/documents/{Guid.NewGuid()}.enc";

        // Generate URL (valid for 15 minutes)
        var url = await _storageService.GenerateUploadPresignedUrlAsync(
            objectKey,
            request.ContentType,
            TimeSpan.FromMinutes(15));

        return Ok(new { UploadUrl = url, ObjectKey = objectKey });
    }

    [HttpPost]
    public async Task<ActionResult<Document>> CreateDocument([FromBody] DocumentDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var document = new Document
        {
            FamilyMemberId = request.FamilyMemberId,
            AccountId = Guid.Parse(User.FindFirst("AccountId")?.Value ?? Guid.Empty.ToString()), // Assuming AccountId is in claims or passed
            DocumentType = request.DocumentType,
            S3BucketName = "digital-vault-documents-local", // Should come from config
            S3ObjectKey = request.S3ObjectKey,
            S3Region = "us-east-1",

            EncryptedOriginalFileName = request.EncryptedOriginalFileName,
            EncryptedFileExtension = request.EncryptedFileExtension,
            EncryptedFileSize = request.EncryptedFileSize,
            EncryptedMimeType = request.EncryptedMimeType,

            EncryptionIV = request.EncryptionIV,
            EncryptionTag = request.EncryptionTag,

            UploadedAt = DateTime.UtcNow
        };

        // If AccountId claim is missing, we might need to fetch it or require it in DTO
        // For now, let's assume valid user context context

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Document>> GetDocument(Guid id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null) return NotFound();

        return document;
    }
}

public class UploadRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class DocumentDto
{
    public Guid FamilyMemberId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string S3ObjectKey { get; set; } = string.Empty;
    public string EncryptedOriginalFileName { get; set; } = string.Empty;
    public string EncryptedFileExtension { get; set; } = string.Empty;
    public string EncryptedFileSize { get; set; } = string.Empty;
    public string EncryptedMimeType { get; set; } = string.Empty;
    public string EncryptionIV { get; set; } = string.Empty;
    public string EncryptionTag { get; set; } = string.Empty;
}
