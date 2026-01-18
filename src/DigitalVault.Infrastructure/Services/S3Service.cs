using Amazon.S3;
using Amazon.S3.Model;
using DigitalVault.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DigitalVault.Infrastructure.Services;

public class S3Service : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:BucketName"] ?? "digital-vault-documents-local";
    }

    public Task<string> GenerateUploadPresignedUrlAsync(string objectKey, string contentType, TimeSpan expiry)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiry),
            ContentType = contentType
        };

        // Enforce server-side encryption
        request.Headers["x-amz-server-side-encryption"] = "AES256";

        return _s3Client.GetPreSignedURLAsync(request);
    }

    public Task<string> GenerateDownloadPresignedUrlAsync(string objectKey, TimeSpan expiry)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiry)
        };

        return _s3Client.GetPreSignedURLAsync(request);
    }

    public async Task DeleteObjectAsync(string objectKey)
    {
        await _s3Client.DeleteObjectAsync(_bucketName, objectKey);
    }
}
