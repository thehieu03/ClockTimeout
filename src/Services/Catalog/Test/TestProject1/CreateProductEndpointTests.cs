using AutoMapper;
using BuildingBlocks.Extensions;
using BuildingBlocks.Authentication.Extensions;
using Catalog.Api.Endpoints;
using Catalog.Api.Models;
using Catalog.Application.Dtos;
using Catalog.Application.Dtos.Products;
using Catalog.Application.Features.Product.Commands;
using Common.Constants;
using Common.Models;
using Common.Models.Context;
using Common.Models.Reponses;
using Common.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reflection;
using System.Security.Claims;

namespace TestProject1;

[TestClass]
public sealed class CreateProductEndpointTests
{
    private Mock<ISender> _mockSender = null!;
    private Mock<IMapper> _mockMapper = null!;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor = null!;
    private Mock<HttpContext> _mockHttpContext = null!;
    private Mock<HttpRequest> _mockRequest = null!;
    private Mock<IFormCollection> _mockForm = null!;
    private CreateProduct _endpoint = null!;
    private MethodInfo _handlerMethod = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _mockMapper = new Mock<IMapper>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockRequest = new Mock<HttpRequest>();
        _mockForm = new Mock<IFormCollection>();

        // Set up default Form mock with empty files collection
        var emptyFiles = new FormFileCollection();
        _mockForm.Setup(x => x.Files).Returns(emptyFiles);
        _mockRequest.Setup(x => x.Form).Returns(_mockForm.Object);
        _mockHttpContext.Setup(x => x.Request).Returns(_mockRequest.Object);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        _endpoint = new CreateProduct();

        // Get private handler method using reflection
        _handlerMethod = typeof(CreateProduct).GetMethod(
            "HandleCreateProductAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [TestMethod]
    public async Task HandleCreateProductAsync_WithValidRequest_ShouldReturnApiCreatedResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Sku = "TEST-SKU-001",
            ShortDescription = "Short description",
            LongDescription = "Long description",
            Price = 99.99m,
            SalePrice = 79.99m,
            Published = true,
            Featured = false
        };

        var dto = new CreateProductDto
        {
            Name = request.Name,
            Sku = request.Sku,
            ShortDescription = request.ShortDescription,
            LongDescription = request.LongDescription,
            Price = request.Price,
            SalePrice = request.SalePrice,
            Published = request.Published,
            Featured = request.Featured
        };

        var userContext = new UserContext
        {
            Email = "test@example.com",
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser"
        };

        _mockHttpContext.Setup(x => x.Request).Returns(_mockRequest.Object);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);
        SetupUserContext(userContext);

        _mockMapper.Setup(x => x.Map<CreateProductDto>(It.IsAny<CreateProductRequest>()))
            .Returns(dto);

        _mockSender.Setup(x => x.Send(
            It.IsAny<CreateProductCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(productId);

        // Act
        var result = await InvokeHandlerAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(productId, result.Value);
        _mockMapper.Verify(x => x.Map<CreateProductDto>(request), Times.Once);
        _mockSender.Verify(x => x.Send(
            It.Is<CreateProductCommand>(cmd =>
                cmd.Dto.Name == request.Name &&
                cmd.Actor.Value == userContext.Email),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleCreateProductAsync_WithNullRequest_ShouldThrowClientValidationException()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act & Assert
        try
        {
            await InvokeHandlerAsync(null!);
            Assert.Fail("Expected ClientValidationException was not thrown.");
        }
        catch (ClientValidationException)
        {
            // Expected exception was thrown
        }
    }

    [TestMethod]
    public async Task HandleCreateProductAsync_WithImageFiles_ShouldProcessImageFiles()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Sku = "TEST-SKU-001",
            ShortDescription = "Short description",
            LongDescription = "Long description",
            Price = 99.99m
        };

        var imageFile1 = CreateMockFormFile("image1.jpg", "image/jpeg", new byte[] { 1, 2, 3 });
        var imageFile2 = CreateMockFormFile("image2.png", "image/png", new byte[] { 4, 5, 6 });
        request.ImageFiles = new List<IFormFile> { imageFile1.Object, imageFile2.Object };

        var dto = new CreateProductDto
        {
            Name = request.Name,
            Sku = request.Sku,
            ShortDescription = request.ShortDescription,
            LongDescription = request.LongDescription,
            Price = request.Price
        };

        var userContext = new UserContext
        {
            Email = "test@example.com",
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser"
        };

        _mockHttpContext.Setup(x => x.Request).Returns(_mockRequest.Object);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);
        SetupUserContext(userContext);

        _mockMapper.Setup(x => x.Map<CreateProductDto>(It.IsAny<CreateProductRequest>()))
            .Returns(dto);

        _mockSender.Setup(x => x.Send(
            It.IsAny<CreateProductCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(productId);

        // Act
        var result = await InvokeHandlerAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(productId, result.Value);
        _mockSender.Verify(x => x.Send(
            It.Is<CreateProductCommand>(cmd =>
                cmd.Dto.UploadImages != null &&
                cmd.Dto.UploadImages.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleCreateProductAsync_WithThumbnailFile_ShouldProcessThumbnail()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Sku = "TEST-SKU-001",
            ShortDescription = "Short description",
            LongDescription = "Long description",
            Price = 99.99m
        };

        var thumbnailFile = CreateMockFormFile("thumbnail.jpg", "image/jpeg", new byte[] { 7, 8, 9 });
        request.ThumbnailFile = thumbnailFile.Object;

        var dto = new CreateProductDto
        {
            Name = request.Name,
            Sku = request.Sku,
            ShortDescription = request.ShortDescription,
            LongDescription = request.LongDescription,
            Price = request.Price
        };

        var userContext = new UserContext
        {
            Email = "test@example.com",
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser"
        };

        _mockHttpContext.Setup(x => x.Request).Returns(_mockRequest.Object);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);
        SetupUserContext(userContext);

        _mockMapper.Setup(x => x.Map<CreateProductDto>(It.IsAny<CreateProductRequest>()))
            .Returns(dto);

        _mockSender.Setup(x => x.Send(
            It.IsAny<CreateProductCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(productId);

        // Act
        var result = await InvokeHandlerAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(productId, result.Value);
        _mockSender.Verify(x => x.Send(
            It.Is<CreateProductCommand>(cmd =>
                cmd.Dto.UploadThumbnail != null &&
                cmd.Dto.UploadThumbnail.FileName == "thumbnail.jpg"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleCreateProductAsync_WithImageFilesFromForm_ShouldUseFormFiles()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Sku = "TEST-SKU-001",
            ShortDescription = "Short description",
            LongDescription = "Long description",
            Price = 99.99m,
            ImageFiles = null // No files in request
        };

        var formFile = CreateMockFormFile("form-image.jpg", "image/jpeg", new byte[] { 10, 11, 12 });
        var formFiles = new FormFileCollection { formFile.Object };

        _mockRequest.Setup(x => x.Form).Returns(new Mock<IFormCollection>().Object);
        _mockRequest.Setup(x => x.Form.Files).Returns(formFiles);

        var dto = new CreateProductDto
        {
            Name = request.Name,
            Sku = request.Sku,
            ShortDescription = request.ShortDescription,
            LongDescription = request.LongDescription,
            Price = request.Price
        };

        var userContext = new UserContext
        {
            Email = "test@example.com",
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser"
        };

        _mockHttpContext.Setup(x => x.Request).Returns(_mockRequest.Object);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);
        SetupUserContext(userContext);

        _mockMapper.Setup(x => x.Map<CreateProductDto>(It.IsAny<CreateProductRequest>()))
            .Returns(dto);

        _mockSender.Setup(x => x.Send(
            It.IsAny<CreateProductCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(productId);

        // Act
        var result = await InvokeHandlerAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(productId, result.Value);
        // Verify that ImageFiles were populated from form
        Assert.IsNotNull(request.ImageFiles);
        Assert.AreEqual(1, request.ImageFiles.Count);
    }

    [TestMethod]
    public async Task HandleCreateProductAsync_WithAllProperties_ShouldMapCorrectly()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var request = new CreateProductRequest
        {
            Name = "Complete Product",
            Sku = "COMPLETE-SKU-001",
            ShortDescription = "Short description",
            LongDescription = "Long description",
            Price = 199.99m,
            SalePrice = 149.99m,
            CategoryIds = new List<string> { categoryId1.ToString(), categoryId2.ToString() },
            BrandId = brandId,
            Colors = new List<string> { "Red", "Blue" },
            Sizes = new List<string> { "S", "M", "L" },
            Tags = new List<string> { "tag1", "tag2" },
            Published = true,
            Featured = true,
            SEOTittle = "SEO Title",
            SEODescription = "SEO Description",
            Barcode = "123456789",
            Unit = "kg",
            Weight = 1.5m
        };

        var dto = new CreateProductDto
        {
            Name = request.Name,
            Sku = request.Sku,
            ShortDescription = request.ShortDescription,
            LongDescription = request.LongDescription,
            Price = request.Price,
            SalePrice = request.SalePrice,
            CategoryIds = request.CategoryIds?.Select(Guid.Parse).ToList(),
            BrandId = request.BrandId,
            Colors = request.Colors,
            Sizes = request.Sizes,
            Tags = request.Tags,
            Published = request.Published,
            Featured = request.Featured,
            SEOTitle = request.SEOTittle,
            SEODescription = request.SEODescription,
            Barcode = request.Barcode,
            Unit = request.Unit,
            Weight = request.Weight
        };

        var userContext = new UserContext
        {
            Email = "test@example.com",
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser"
        };

        _mockHttpContext.Setup(x => x.Request).Returns(_mockRequest.Object);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);
        SetupUserContext(userContext);

        _mockMapper.Setup(x => x.Map<CreateProductDto>(It.IsAny<CreateProductRequest>()))
            .Returns(dto);

        _mockSender.Setup(x => x.Send(
            It.IsAny<CreateProductCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(productId);

        // Act
        var result = await InvokeHandlerAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(productId, result.Value);
        _mockSender.Verify(x => x.Send(
            It.Is<CreateProductCommand>(cmd =>
                cmd.Dto.Name == "Complete Product" &&
                cmd.Dto.CategoryIds != null &&
                cmd.Dto.CategoryIds.Count == 2 &&
                cmd.Dto.Colors != null &&
                cmd.Dto.Colors.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private async Task<ApiCreatedResponse<Guid>> InvokeHandlerAsync(CreateProductRequest request)
    {
        return await (Task<ApiCreatedResponse<Guid>>)_handlerMethod.Invoke(
            _endpoint,
            new object[]
            {
                _mockSender.Object,
                _mockMapper.Object,
                _mockHttpContextAccessor.Object,
                request
            })!;
    }

    private Mock<IFormFile> CreateMockFormFile(string fileName, string contentType, byte[] content)
    {
        var mockFile = new Mock<IFormFile>();
        var ms = new MemoryStream(content);
        mockFile.Setup(x => x.FileName).Returns(fileName);
        mockFile.Setup(x => x.ContentType).Returns(contentType);
        mockFile.Setup(x => x.Length).Returns(content.Length);
        mockFile.Setup(x => x.OpenReadStream()).Returns(ms);
        mockFile.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream target, CancellationToken ct) => ms.CopyToAsync(target, ct));
        return mockFile;
    }

    private void SetupUserContext(UserContext userContext)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userContext.Id),
            new Claim(ClaimTypes.Email, userContext.Email),
            new Claim(CustomClaimTypes.UserName, userContext.UserName ?? string.Empty)
        };

        if (!string.IsNullOrEmpty(userContext.FirstName))
            claims.Add(new Claim(ClaimTypes.GivenName, userContext.FirstName));
        if (!string.IsNullOrEmpty(userContext.LastName))
            claims.Add(new Claim(ClaimTypes.Surname, userContext.LastName));
        if (!string.IsNullOrEmpty(userContext.Tenant))
            claims.Add(new Claim(CustomClaimTypes.Tenant, userContext.Tenant));
        if (userContext.EmailVerified)
            claims.Add(new Claim(CustomClaimTypes.EmailVerified, "true"));
        if (userContext.Roles != null)
        {
            foreach (var role in userContext.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);
    }
}
