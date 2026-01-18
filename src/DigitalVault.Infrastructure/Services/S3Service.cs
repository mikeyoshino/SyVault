using Amazon.S3;
using Amazon.S3.Model;
using DigitalVault.Application.Interfaces;
using Microsoft.Extensions.Options;
using DigitalVault.Infrastructure.Configuration;

namespace DigitalVault.Infrastructure.Services;

public class S3Service : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IAmazonS3 s3Client, IOptions<AwsSettings> settings)
    {
        _s3Client = s3Client;
        _bucketName = settings.Value.BucketName;
    }

    public Task<string> GenerateUploadPresignedUrlAsync(string objectKey, string contentType, TimeSpan expiry)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiry),
            ContentType = contentType,
            Protocol = Protocol.HTTP // CRITICAL: Force HTTP for MinIO
        };

        // Note: Server-side encryption removed for MinIO compatibility in dev
        // request.Headers["x-amz-server-side-encryption"] = "AES256";

        return _s3Client.GetPreSignedURLAsync(request);
    }

    public Task<string> GenerateDownloadPresignedUrlAsync(string objectKey, TimeSpan expiry)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiry),
            Protocol = Protocol.HTTP // Force HTTP for MinIO
        };

        return _s3Client.GetPreSignedURLAsync(request);
    }

    public async Task DeleteObjectAsync(string objectKey)
    {
        await _s3Client.DeleteObjectAsync(_bucketName, objectKey);
    }
}
