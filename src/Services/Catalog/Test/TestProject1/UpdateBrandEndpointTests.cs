using AutoMapper;
using BuildingBlocks.Authentication.Extensions;
using BuildingBlocks.Extensions;
using Catalog.Api.Endpoints;
using Catalog.Api.Models;
using Catalog.Application.Dtos.Brands;
using Catalog.Application.Features.Brand.Commands;
using Common.Constants;
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
public sealed class UpdateBrandEndpointTests
{
    private Mock<ISender> _mockSender = null!;
    private Mock<IMapper> _mockMapper = null!;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor = null!;
    private Mock<HttpContext> _mockHttpContext = null!;
    private UpdateBrand _endpoint = null!;
    private MethodInfo _handlerMethod = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _mockMapper = new Mock<IMapper>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHttpContext = new Mock<HttpContext>();

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        _endpoint = new UpdateBrand();

        // Get private handler method using reflection
        _handlerMethod = typeof(UpdateBrand).GetMethod(
            "HandleUpdateBrandAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [TestMethod]
    public async Task HandleUpdateBrandAsync_WithValidRequest_ShouldReturnApiUpdatedResponse()
    {
        // Arrange
        var brandId = Guid.NewGuid();
        var request = new UpdateBrandRequest
        {
            Name = "Nike Updated"
        };

        var dto = new UpdateBrandDto
        {
            Name = request.Name
        };

        var userContext = new UserContext
        {
            Email = "test@example.com",
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser"
        };

        SetupUserContext(userContext);

        _mockMapper.Setup(x => x.Map<UpdateBrandDto>(It.IsAny<UpdateBrandRequest>()))
            .Returns(dto);

        _mockSender.Setup(x => x.Send(
            It.IsAny<UpdateBrandCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(brandId);

        // Act
        var result = await InvokeHandlerAsync(brandId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(brandId, result.Value);
        _mockMapper.Verify(x => x.Map<UpdateBrandDto>(request), Times.Once);
        _mockSender.Verify(x => x.Send(
            It.Is<UpdateBrandCommand>(cmd =>
                cmd.BrandId == brandId &&
                cmd.Dto.Name == request.Name &&
                cmd.Actor.Value == userContext.Email),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleUpdateBrandAsync_WithNullRequest_ShouldThrowException()
    {
        // Arrange
        var brandId = Guid.NewGuid();
        UpdateBrandRequest? request = null;

        // Act & Assert
        try
        {
            await InvokeHandlerAsync(brandId, request!);
            Assert.Fail("Expected ClientValidationException was not thrown.");
        }
        catch (ClientValidationException)
        {
            // Expected exception was thrown
        }
    }

    private void SetupUserContext(UserContext userContext)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, userContext.Email),
            new Claim(ClaimTypes.NameIdentifier, userContext.Id),
            new Claim(ClaimTypes.Name, userContext.UserName ?? "")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _mockHttpContext.Setup(x => x.User).Returns(principal);
    }

    private async Task<ApiUpdatedResponse<Guid>> InvokeHandlerAsync(Guid brandId, UpdateBrandRequest request)
    {
        return await (Task<ApiUpdatedResponse<Guid>>)_handlerMethod.Invoke(
            _endpoint,
            new object[]
            {
                _mockSender.Object,
                _mockMapper.Object,
                _mockHttpContextAccessor.Object,
                brandId,
                request
            })!;
    }
}
