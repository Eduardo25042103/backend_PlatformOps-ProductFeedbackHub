namespace OpsHub.Application.Common.Interfaces;

public interface IS3StorageService
{
    string GeneratePreSignedUploadUrl(string fileName, string contentType, out string s3Key, int expirationMinutes = 5);
    string GeneratePreSignedDownloadUrl(string s3Key, int expirationMinutes = 60);
}
