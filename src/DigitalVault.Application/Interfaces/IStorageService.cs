using System.Threading.Tasks;

namespace DigitalVault.Application.Interfaces;

public interface IStorageService
{
    Task<string> GenerateUploadPresignedUrlAsync(string objectKey, string contentType, TimeSpan expiry);
    Task<string> GenerateDownloadPresignedUrlAsync(string objectKey, TimeSpan expiry);
    Task DeleteObjectAsync(string objectKey);
}
