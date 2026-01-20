namespace Catalog.Application.Services;

public interface IMinlOCloundServices
{
    #region Methods
    Task<List<UploadFileResult>> UploadFilesAsync(List<UploadFileBytes> files, string bucketName,
        bool isPublicBuket = false, CancellationToken ct=default);
    Task<string> GetShareLinkAsync(string buketName, string objectName, int expireTime);
    #endregion
}