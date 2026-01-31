namespace Catalog.Application.Services;

public interface IMinIOCloudService
{
    Task<List<UploadFileResult>> UploadFilesAsync(List<UploadFileBytes> files, string bucketName,
        bool isPublicBuket = false, CancellationToken ct=default);
    Task<string> GetShareLinkAsync(string buketName, string objectName, int expireTime);
}