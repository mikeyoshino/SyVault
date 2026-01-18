using DigitalVault.Application.Interfaces;
using DigitalVault.Domain.Entities;
using DigitalVault.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DigitalVault.Logic.Services;

public class DocumentService
{
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DocumentService> _logger;
    private readonly AwsSettings _awsSettings;

    public DocumentService(
        IStorageService storageService,
        IUnitOfWork unitOfWork,
        ILogger<DocumentService> logger,
        IOptions<AwsSettings> awsSettings)
    {
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _awsSettings = awsSettings.Value;
    }

    public async Task<(string UploadUrl, string ObjectKey)> GenerateUploadUrlAsync(string contentType, Guid userId)
    {
        var objectKey = $"accounts/{userId}/documents/{Guid.NewGuid()}.enc";

        var url = await _storageService.GenerateUploadPresignedUrlAsync(
            objectKey,
            contentType,
            TimeSpan.FromMinutes(15));

        _logger.LogInformation("Generated presigned URL: {Url}", url);

        return (url, objectKey);
    }

    public async Task<Document> CreateDocumentAsync(Document document)
    {
        // Enforce AWS config values here to ensure consistency
        document.S3BucketName = _awsSettings.BucketName;
        document.S3Region = _awsSettings.Region;
        document.UploadedAt = DateTime.UtcNow;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _unitOfWork.Documents.AddAsync(document);
            await _unitOfWork.CompleteAsync();
        });

        return document;
    }

    public async Task<Document?> GetDocumentAsync(Guid id)
    {
        return await _unitOfWork.Documents.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Document>> GetDocumentsByFamilyMemberAsync(Guid familyMemberId, Guid userId)
    {
        return await _unitOfWork.Documents.FindAsync(d => d.FamilyMemberId == familyMemberId && d.AccountId == userId);
    }

    public async Task<string> GeneratePreviewUrlAsync(Guid documentId, Guid userId)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
        if (document == null || document.AccountId != userId)
        {
            throw new KeyNotFoundException("Document not found or access denied.");
        }

        return await _storageService.GenerateDownloadPresignedUrlAsync(
            document.S3ObjectKey,
            TimeSpan.FromMinutes(15));
    }

    public async Task DeleteDocumentAsync(Guid documentId, Guid userId)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
        if (document == null || document.AccountId != userId)
        {
            throw new KeyNotFoundException("Document not found or access denied.");
        }

        // 1. Delete from S3
        await _storageService.DeleteObjectAsync(document.S3ObjectKey);

        // 2. Delete from DB (Transactional)
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            _unitOfWork.Documents.Remove(document);
            await _unitOfWork.CompleteAsync();
        });
    }
}
