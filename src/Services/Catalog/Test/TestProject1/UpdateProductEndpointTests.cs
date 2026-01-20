using AutoMapper;
using BuildingBlocks.Extensions;
using BuildingBlocks.Authentication.Extensions;
using Catalog.Api.Endpoints;
using Catalog.Api.Models;
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
public sealed class UpdateProductEndpointTests
{
    private Mock<ISender> _mockSender = null!;
    private Mock<IMapper> _mockMapper = null!;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor = null!;
    private Mock<HttpContext> _mockHttpContext = null!;
    private Mock<HttpRequest> _mockRequest = null!;
    private Mock<IFormCollection> _mockForm = null!;
    private UpdateProduct _endpoint = null!;
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

        _endpoint = new UpdateProduct();

        // Get private handler method using reflection
        _handlerMethod = typeof(UpdateProduct).GetMethod(
            "HandleUpdateProductAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [TestMethod]
    public async Task HandleUpdateProductAsync_WithValidRequest_ShouldReturnApiUpdatedResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var request = new UpdateProductRequest
        {
            Name = "Updated Product",
            Sku = "UPDATED-SKU-001",
            ShortDescription = "Updated short description",
            LongDescription = "Updated long description with more details",
            Price = 149.99m,
            SalePrice = 129.99m,
            CategoryIds = new List<string> { categoryId1.ToString(), categoryId2.ToString() },
            BrandId = brandId,
            Colors = new List<string> { "Red", "Blue", "Green" },
            Sizes = new List<string> { "S", "M", "L", "XL" },
            Tags = new List<string> { "updated", "featured" },
            Published = true,
            Featured = true,
            SEOTittle = "Updated SEO Title",
            SEODescription = "Updated SEO Description",
            Barcode = "987654321",
            Unit = "piece",
            Weight = 2.5m
        };

        var dto = new UpdateProductDto
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

        _mockMapper.Setup(x => x.Map<UpdateProductDto>(It.IsAny<UpdateProductRequest>()))
            .Returns(dto);

        _mockSender.Setup(x => x.Send(
            It.IsAny<UpdateProductCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(productId);

        // Act
        var result = await InvokeHandlerAsync(productId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(productId, result.Value);
        _mockMapper.Verify(x => x.Map<UpdateProductDto>(request), Times.Once);
        _mockSender.Verify(x => x.Send(
            It.Is<UpdateProductCommand>(cmd =>
                cmd.ProductId == productId &&
                cmd.Dto.Name == request.Name &&
                cmd.Dto.Sku == request.Sku &&
                cmd.Dto.CategoryIds != null &&
                cmd.Dto.CategoryIds.Count == 2 &&
                cmd.Actor.Value == userContext.Email),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleUpdateProductAsync_WithNullRequest_ShouldThrowClientValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act & Assert
        try
        {
            await InvokeHandlerAsync(productId, null!);
            Assert.Fail("Expected ClientValidationException was not thrown.");
        }
        catch (ClientValidationException)
        {
            // Expected exception was thrown
        }
    }

    private async Task<ApiUpdatedResponse<Guid>> InvokeHandlerAsync(
        Guid productId,
        UpdateProductRequest request)
    {
        return await (Task<ApiUpdatedResponse<Guid>>)_handlerMethod.Invoke(
            _endpoint,
            new object[]
            {
                _mockSender.Object,
                _mockMapper.Object,
                _mockHttpContextAccessor.Object,
                productId,
                request
            })!;
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
