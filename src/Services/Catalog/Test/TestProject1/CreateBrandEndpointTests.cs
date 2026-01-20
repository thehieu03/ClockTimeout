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
public sealed class CreateBrandEndpointTests
{
    private Mock<ISender> _mockSender = null!;
    private Mock<IMapper> _mockMapper = null!;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor = null!;
    private Mock<HttpContext> _mockHttpContext = null!;
    private CreateBrand _endpoint = null!;
    private MethodInfo _handlerMethod = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _mockMapper = new Mock<IMapper>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHttpContext = new Mock<HttpContext>();

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        _endpoint = new CreateBrand();

        // Get private handler method using reflection
        _handlerMethod = typeof(CreateBrand).GetMethod(
            "HandleCreateBrandAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [TestMethod]
    public async Task HandleCreateBrandAsync_WithValidRequest_ShouldReturnApiCreatedResponse()
    {
        // Arrange
        var brandId = Guid.NewGuid();
        var request = new CreateBrandRequest
        {
            Name = "Nike"
        };

        var dto = new CreateBrandDto
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

        _mockMapper.Setup(x => x.Map<CreateBrandDto>(It.IsAny<CreateBrandRequest>()))
            .Returns(dto);

        _mockSender.Setup(x => x.Send(
            It.IsAny<CreateBrandCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(brandId);

        // Act
        var result = await InvokeHandlerAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(brandId, result.Value);
        _mockMapper.Verify(x => x.Map<CreateBrandDto>(request), Times.Once);
        _mockSender.Verify(x => x.Send(
            It.Is<CreateBrandCommand>(cmd =>
                cmd.Dto.Name == request.Name &&
                cmd.Actor.Value == userContext.Email),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleCreateBrandAsync_WithNullRequest_ShouldThrowException()
    {
        // Arrange
        CreateBrandRequest? request = null;

        // Act & Assert
        try
        {
            await InvokeHandlerAsync(request!);
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

    private async Task<ApiCreatedResponse<Guid>> InvokeHandlerAsync(CreateBrandRequest request)
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
}
