using AutoMapper;
using BuildingBlocks.Authentication.Extensions;
using BuildingBlocks.Extensions;
using Catalog.Api.Endpoints;
using Catalog.Api.Models;
using Catalog.Application.Dtos.Categories;
using Catalog.Application.Features.Category.Commands;
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
public sealed class UpdateCategoryEndpointTests
{
    private Mock<ISender> _mockSender = null!;
    private Mock<IMapper> _mockMapper = null!;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor = null!;
    private Mock<HttpContext> _mockHttpContext = null!;
    private UpdateCategory _endpoint = null!;
    private MethodInfo _handlerMethod = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _mockMapper = new Mock<IMapper>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockHttpContext = new Mock<HttpContext>();

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        _endpoint = new UpdateCategory();

        // Get private handler method using reflection
        _handlerMethod = typeof(UpdateCategory).GetMethod(
            "HandleUpdateCategoryAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [TestMethod]
    public async Task HandleUpdateCategoryAsync_WithValidRequest_ShouldReturnApiUpdatedResponse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new UpdateCategoryRequest
        {
            Name = "Updated Electronics",
            Description = "Updated description for electronics",
            ParentId = null
        };

        var dto = new UpdateCategoryDto
        {
            Name = request.Name,
            Description = request.Description,
            ParentId = request.ParentId
        };

        var userContext = new UserContext
        {
            Email = "test@example.com",
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser"
        };

        SetupUserContext(userContext);

        _mockMapper.Setup(x => x.Map<UpdateCategoryDto>(It.IsAny<UpdateCategoryRequest>()))
            .Returns(dto);

        _mockSender.Setup(x => x.Send(
            It.IsAny<UpdateCategoryCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryId);

        // Act
        var result = await InvokeHandlerAsync(categoryId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(categoryId, result.Value);
        _mockMapper.Verify(x => x.Map<UpdateCategoryDto>(request), Times.Once);
        _mockSender.Verify(x => x.Send(
            It.Is<UpdateCategoryCommand>(cmd =>
                cmd.CategoryId == categoryId &&
                cmd.Dto.Name == request.Name &&
                cmd.Dto.Description == request.Description &&
                cmd.Actor.Value == userContext.Email),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleUpdateCategoryAsync_WithParentId_ShouldUpdateWithParent()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var request = new UpdateCategoryRequest
        {
            Name = "Updated Smartphones",
            Description = "Updated description",
            ParentId = parentId
        };

        var dto = new UpdateCategoryDto
        {
            Name = request.Name,
            Description = request.Description,
            ParentId = request.ParentId
        };

        var userContext = new UserContext
        {
            Email = "test@example.com",
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser"
        };

        SetupUserContext(userContext);

        _mockMapper.Setup(x => x.Map<UpdateCategoryDto>(It.IsAny<UpdateCategoryRequest>()))
            .Returns(dto);

        _mockSender.Setup(x => x.Send(
            It.IsAny<UpdateCategoryCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryId);

        // Act
        var result = await InvokeHandlerAsync(categoryId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(categoryId, result.Value);
        _mockSender.Verify(x => x.Send(
            It.Is<UpdateCategoryCommand>(cmd =>
                cmd.Dto.ParentId == parentId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleUpdateCategoryAsync_WithNullRequest_ShouldThrowClientValidationException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        // Act & Assert
        try
        {
            await InvokeHandlerAsync(categoryId, null!);
            Assert.Fail("Expected ClientValidationException was not thrown.");
        }
        catch (ClientValidationException)
        {
            // Expected exception was thrown
        }
    }

    private async Task<ApiUpdatedResponse<Guid>> InvokeHandlerAsync(
        Guid categoryId,
        UpdateCategoryRequest request)
    {
        return await (Task<ApiUpdatedResponse<Guid>>)_handlerMethod.Invoke(
            _endpoint,
            new object[]
            {
                _mockSender.Object,
                _mockMapper.Object,
                _mockHttpContextAccessor.Object,
                categoryId,
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
