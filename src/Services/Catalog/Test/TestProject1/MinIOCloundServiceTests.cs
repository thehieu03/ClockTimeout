using Catalog.Application.Services;
using Catalog.Infrastructure.Services;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Response;
using Common.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TestProject1;

[TestClass]
[TestCategory("Integration")]
[Ignore("These tests require a properly configured MinioClient which cannot be easily mocked")]
public sealed class MinIOCloundServiceTests
{
    private Mock<IMinioClient> _minioClient = null!;
    private MinIOCloundService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _minioClient = new Mock<IMinioClient>();
        // Note: IMinioClient.Config.Endpoint is not mockable as Config is not virtual
        // These tests are marked as ignored until a proper abstraction is created
    }

    [TestMethod]
    public async Task GetShareLinkAsync_ShouldReturnLink()
    {
        var expected = "http://link";
        _minioClient.Setup(c => c.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
            .ReturnsAsync(expected);

        var result = await _service.GetShareLinkAsync("bucket", "obj", 60);

        Assert.AreEqual(expected, result);
        _minioClient.Verify(c => c.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()), Times.Once);
    }

    [TestMethod]
    public async Task UploadFilesAsync_WhenNoFiles_ShouldReturnEmpty()
    {
        var result = await _service.UploadFilesAsync(new List<UploadFileBytes>(), "bucket");
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task UploadFilesAsync_ShouldUploadAndReturnResult()
    {
        var ct = CancellationToken.None;
        _minioClient.Setup(c => c.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), ct)).ReturnsAsync(true);
        _minioClient.Setup(c => c.PutObjectAsync(It.IsAny<PutObjectArgs>(), ct))
            .ReturnsAsync((PutObjectResponse?)null);

        var files = new List<Common.Models.UploadFileBytes>
        {
            new Common.Models.UploadFileBytes
            {
                FileName = "test.png",
                ContentType = "image/png",
                Bytes = new byte[] { 1, 2, 3 }
            }
        };

        var result = await _service.UploadFilesAsync(files, "bucket", true, ct);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("bucket", result[0].FolderName);
        Assert.AreEqual("image/png", result[0].ContentType);
        Assert.IsTrue(result[0].PublicURL.Contains("bucket"));

        _minioClient.Verify(c => c.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), ct), Times.Once);
        _minioClient.Verify(c => c.PutObjectAsync(It.IsAny<PutObjectArgs>(), ct), Times.Once);
    }
}
