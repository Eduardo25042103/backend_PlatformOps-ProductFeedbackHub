using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpsHub.Application.Common.Interfaces;

namespace OpsHub.Infrastructure.Services;

public class S3StorageService : IS3StorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3StorageService> _logger;
    private readonly string _bucketName;
    private readonly string _region;

    public S3StorageService(IConfiguration configuration, ILogger<S3StorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _bucketName = _configuration["AWS:BucketName"] ?? "opshub-deploy-idat-2026";
        _region = _configuration["AWS:Region"] ?? Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-2";
    }

    public string GeneratePreSignedUploadUrl(string fileName, string contentType, out string s3Key, int expirationMinutes = 5)
    {
        var cleanFileName = Path.GetFileName(fileName).Replace(" ", "_");
        s3Key = $"tickets/{DateTime.UtcNow:yyyy-MM}/{Guid.NewGuid()}_{cleanFileName}";

        try
        {
            var regionEndpoint = RegionEndpoint.GetBySystemName(_region);
            using var s3Client = new AmazonS3Client(regionEndpoint);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = s3Key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                ContentType = contentType
            };

            return s3Client.GetPreSignedURL(request);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AWS SDK no pudo firmar con credenciales reales en local. Generando URL simulada para entorno de pruebas.");
            // En desarrollo local sin credenciales de AWS activas, retornamos URL representativa
            return $"https://{_bucketName}.s3.{_region}.amazonaws.com/{s3Key}?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Expires={expirationMinutes * 60}&mock=true";
        }
    }

    public string GeneratePreSignedDownloadUrl(string s3Key, int expirationMinutes = 60)
    {
        try
        {
            var regionEndpoint = RegionEndpoint.GetBySystemName(_region);
            using var s3Client = new AmazonS3Client(regionEndpoint);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = s3Key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            return s3Client.GetPreSignedURL(request);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AWS SDK no pudo firmar URL de descarga en local.");
            return $"https://{_bucketName}.s3.{_region}.amazonaws.com/{s3Key}?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Expires={expirationMinutes * 60}&mock=true";
        }
    }
}
