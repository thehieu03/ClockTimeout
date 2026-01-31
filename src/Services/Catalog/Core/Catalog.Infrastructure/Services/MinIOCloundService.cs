using Catalog.Application.Services;
using Catalog.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Minio;
using Minio.DataModel.Args;
using System.Text.Json;

namespace Catalog.Infrastructure.Services;

public class MinIOCloundService : IMinIOCloudService
{
    #region prop
    private readonly IMinioClient _minioClient;
    private readonly string _endPoint;
    #endregion

    #region ctors
    public MinIOCloundService(IMinioClient minioClient)
    {
        _minioClient = minioClient;
        _endPoint = _minioClient.Config.Endpoint;
    }
    #endregion
    #region Implementations
    public async Task<string> GetShareLinkAsync(string buketName, string objectName, int expireTime)
    {
        try
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(buketName)
                .WithObject(objectName)
                .WithExpiry(expireTime * 60);
            return await _minioClient.PresignedGetObjectAsync(args);
        }
        catch (Exception ex)
        {
            throw new InfrastructureException(ex.Message);
        }
    }

    public async Task<List<UploadFileResult>> UploadFilesAsync(List<UploadFileBytes> files, string bucketName, bool isPublicBuket = false, CancellationToken ct = default)
    {
        var results = new List<UploadFileResult>();
        if (files is null || files.Count == 0) return results;

        try
        {
            await EnsureBucketAsync(bucketName, isPublicBuket, ct);
            foreach (var f in files)
            {
                var fileCloundId = Guid.NewGuid();
                var ext = Path.GetExtension(f.FileName);
                var objectName = $"{fileCloundId:N}{ext}";
                using var stream = new MemoryStream(f.Bytes, 0, f.Bytes.Length, writable: false, publiclyVisible: true);
                var putArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(f.ContentType);
                var putResp = await _minioClient.PutObjectAsync(putArgs, ct);
                results.Add(new UploadFileResult
                {
                    FileId = fileCloundId.ToString(),
                    FolderName = bucketName,
                    OriginalFileName = f.FileName,
                    FileName = objectName,
                    FileSize = f.Bytes.LongLength,
                    ContentType = f.ContentType,
                    PublicURL = isPublicBuket ? $"{_endPoint}/{bucketName}/{objectName}" : string.Empty
                });
            }
            return results;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException(ex.Message);
        }


    }

    private async Task EnsureBucketAsync(string bucketName, bool isPublicBuket, CancellationToken ct)
    {
        var exists = await _minioClient
            .BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName), ct);
        if (!exists)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName), ct);
            if (isPublicBuket)
            {
                // set read-only bucket policy
                var policy = new
                {
                    Version = "2026-1-21",
                    Statement = new[]
                    {
                        new
                        {
                            Effect="Allow",
                            Principal="*",
                            Action=new [] {"s3:GetObject"},
                            Resource=$"arn:aws:s3:::{bucketName}/"
                        }
                    }
                };
                var policyJson = JsonSerializer.Serialize(policy);
                await _minioClient.SetPolicyAsync(
                    new SetPolicyArgs().WithBucket(bucketName).WithPolicy(policyJson), ct);
            }

        }
    }
    #endregion
}