namespace DigitalVault.Infrastructure.Configuration;

public class AwsSettings
{
    public const string SectionName = "AWS";

    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string ServiceURL { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool ForcePathStyle { get; set; }
}
