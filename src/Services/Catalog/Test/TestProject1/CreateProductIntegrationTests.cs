using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using BuildingBlocks.Extensions;
using Catalog.Api.Constants;
using Catalog.Api.Models;
using Common.Constants;
using Common.Models.Context;
using Common.Models.Reponses;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

/// <summary>
/// Integration tests for CreateProduct endpoint (Day 23)
/// These tests require:
/// 1. PostgreSQL database running (docker-compose up postgres-sql)
/// 2. Catalog API service running (dotnet run in Catalog.Api project)
/// 3. Database migrations applied
/// 
/// To run these tests:
/// 1. Start infrastructure: docker-compose -f docker-compose.infrastructure.yml up -d postgres-sql
/// 2. Start API service: cd src/Services/Catalog/Api/Catalog.Api && dotnet run
/// 3. Run tests: dotnet test --filter "FullyQualifiedName~CreateProductIntegrationTests"
/// </summary>
[TestClass]
public sealed class CreateProductIntegrationTests
{
    private static HttpClient? _httpClient;
    private const string BaseUrl = "http://localhost:5000"; // Change if your API runs on different port
    private const string ApiEndpoint = $"{BaseUrl}{ApiRoutes.Product.Create}";

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _httpClient?.Dispose();
    }

    #region Test Case 1: CreateProduct with Valid Data (No Files)

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Day23")]
    public async Task CreateProduct_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = $"iPhone 15 Pro Max - {Guid.NewGuid()}",
            Sku = $"IPHONE-15-PRO-MAX-{Guid.NewGuid():N}",
            ShortDescription = "Latest iPhone with A17 Pro chip",
            LongDescription = "The iPhone 15 Pro Max features the powerful A17 Pro chip, advanced camera system, and titanium design.",
            Price = 1199.99m,
            SalePrice = 1099.99m,
            Published = false,
            Featured = false
        };

        var content = CreateMultipartFormContent(request);

        // Act
        var response = await _httpClient!.PostAsync(ApiEndpoint, content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode,
            $"Expected 201 Created but got {response.StatusCode}. Response: {await response.Content.ReadAsStringAsync()}");

        var result = await response.Content.ReadFromJsonAsync<ApiCreatedResponse<Guid>>();
        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result.Value);
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Day23")]
    public async Task CreateProduct_WithAllProperties_ShouldReturn201Created()
    {
        // Arrange
        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var request = new CreateProductRequest
        {
            Name = $"Complete Product - {Guid.NewGuid()}",
            Sku = $"COMPLETE-SKU-{Guid.NewGuid():N}",
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

        var content = CreateMultipartFormContent(request);

        // Act
        var response = await _httpClient!.PostAsync(ApiEndpoint, content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiCreatedResponse<Guid>>();
        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result.Value);
    }

    #endregion

    #region Test Case 2: CreateProduct with File Upload

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Day23")]
    public async Task CreateProduct_WithImageFiles_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = $"Product with Images - {Guid.NewGuid()}",
            Sku = $"PRODUCT-IMAGES-{Guid.NewGuid():N}",
            ShortDescription = "Product with images",
            LongDescription = "Long description",
            Price = 99.99m
        };

        var content = CreateMultipartFormContent(request);

        // Add image files
        var image1Bytes = Encoding.UTF8.GetBytes("Fake image 1 content");
        var image2Bytes = Encoding.UTF8.GetBytes("Fake image 2 content");

        content.Add(new ByteArrayContent(image1Bytes), "ImageFiles", "image1.jpg");
        content.Add(new ByteArrayContent(image2Bytes), "ImageFiles", "image2.png");

        // Act
        var response = await _httpClient!.PostAsync(ApiEndpoint, content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiCreatedResponse<Guid>>();
        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result.Value);
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Day23")]
    public async Task CreateProduct_WithThumbnailFile_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = $"Product with Thumbnail - {Guid.NewGuid()}",
            Sku = $"PRODUCT-THUMBNAIL-{Guid.NewGuid():N}",
            ShortDescription = "Product with thumbnail",
            LongDescription = "Long description",
            Price = 99.99m
        };

        var content = CreateMultipartFormContent(request);

        // Add thumbnail file
        var thumbnailBytes = Encoding.UTF8.GetBytes("Fake thumbnail content");
        content.Add(new ByteArrayContent(thumbnailBytes), "ThumbnailFile", "thumbnail.jpg");

        // Act
        var response = await _httpClient!.PostAsync(ApiEndpoint, content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiCreatedResponse<Guid>>();
        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result.Value);
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Day23")]
    public async Task CreateProduct_WithAllFiles_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = $"Product with All Files - {Guid.NewGuid()}",
            Sku = $"PRODUCT-ALL-FILES-{Guid.NewGuid():N}",
            ShortDescription = "Product with all files",
            LongDescription = "Long description",
            Price = 99.99m
        };

        var content = CreateMultipartFormContent(request);

        // Add image files
        content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("Image 1")), "ImageFiles", "image1.jpg");
        content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("Image 2")), "ImageFiles", "image2.jpg");

        // Add thumbnail
        content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("Thumbnail")), "ThumbnailFile", "thumbnail.jpg");

        // Act
        var response = await _httpClient!.PostAsync(ApiEndpoint, content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiCreatedResponse<Guid>>();
        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result.Value);
    }

    #endregion

    #region Test Case 3: Validation Errors

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Day23")]
    public async Task CreateProduct_WithMissingName_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = string.Empty, // Empty name
            Sku = $"TEST-SKU-{Guid.NewGuid():N}",
            ShortDescription = "Description",
            LongDescription = "Long description",
            Price = 100.00m
        };

        var content = CreateMultipartFormContent(request);

        // Act
        var response = await _httpClient!.PostAsync(ApiEndpoint, content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Day23")]
    public async Task CreateProduct_WithMissingSku_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = $"Test Product - {Guid.NewGuid()}",
            Sku = string.Empty, // Empty SKU
            ShortDescription = "Description",
            LongDescription = "Long description",
            Price = 100.00m
        };

        var content = CreateMultipartFormContent(request);

        // Act
        var response = await _httpClient!.PostAsync(ApiEndpoint, content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Day23")]
    public async Task CreateProduct_WithInvalidPrice_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = $"Test Product - {Guid.NewGuid()}",
            Sku = $"TEST-SKU-{Guid.NewGuid():N}",
            ShortDescription = "Description",
            LongDescription = "Long description",
            Price = -100.00m // Invalid negative price
        };

        var content = CreateMultipartFormContent(request);

        // Act
        var response = await _httpClient!.PostAsync(ApiEndpoint, content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Day23")]
    public async Task CreateProduct_WithDuplicateSku_ShouldReturn400BadRequest()
    {
        // Arrange - Create first product
        var sku = $"DUPLICATE-SKU-{Guid.NewGuid():N}";
        var request1 = new CreateProductRequest
        {
            Name = $"First Product - {Guid.NewGuid()}",
            Sku = sku,
            ShortDescription = "Description",
            LongDescription = "Long description",
            Price = 100.00m
        };

        var content1 = CreateMultipartFormContent(request1);
        var response1 = await _httpClient!.PostAsync(ApiEndpoint, content1);
        Assert.AreEqual(HttpStatusCode.Created, response1.StatusCode, "First product should be created successfully");

        // Act - Try to create second product with same SKU
        var request2 = new CreateProductRequest
        {
            Name = $"Second Product - {Guid.NewGuid()}",
            Sku = sku, // Same SKU
            ShortDescription = "Description",
            LongDescription = "Long description",
            Price = 100.00m
        };

        var content2 = CreateMultipartFormContent(request2);
        var response2 = await _httpClient!.PostAsync(ApiEndpoint, content2);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response2.StatusCode,
            "Duplicate SKU should return 400 Bad Request");
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Day23")]
    public async Task CreateProduct_WithNullRequest_ShouldReturn400BadRequest()
    {
        // Arrange
        var content = new MultipartFormDataContent();

        // Act - Send empty request
        var response = await _httpClient!.PostAsync(ApiEndpoint, content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Test Case 4: Edge Cases

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Day23")]
    public async Task CreateProduct_WithVeryLongName_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = new string('A', 300), // Very long name (> 200 chars)
            Sku = $"TEST-SKU-{Guid.NewGuid():N}",
            ShortDescription = "Description",
            LongDescription = "Long description",
            Price = 100.00m
        };

        var content = CreateMultipartFormContent(request);

        // Act
        var response = await _httpClient!.PostAsync(ApiEndpoint, content);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
            "Very long name should be rejected by validation");
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Day23")]
    public async Task CreateProduct_WithSpecialCharacters_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = $"Product with special chars: <>&\"' - {Guid.NewGuid()}",
            Sku = $"TEST-SKU-{Guid.NewGuid():N}",
            ShortDescription = "Description",
            LongDescription = "Long description",
            Price = 100.00m
        };

        var content = CreateMultipartFormContent(request);

        // Act
        var response = await _httpClient!.PostAsync(ApiEndpoint, content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode,
            "Special characters should be handled correctly");
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Day23")]
    public async Task CreateProduct_WithNullOptionalFields_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = $"Product with nulls - {Guid.NewGuid()}",
            Sku = $"TEST-SKU-{Guid.NewGuid():N}",
            ShortDescription = "Description",
            LongDescription = "Long description",
            Price = 100.00m,
            SalePrice = null,
            CategoryIds = null,
            BrandId = null,
            Colors = null,
            Sizes = null,
            Tags = null
        };

        var content = CreateMultipartFormContent(request);

        // Act
        var response = await _httpClient!.PostAsync(ApiEndpoint, content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode,
            "Null optional fields should be accepted");
    }

    [TestMethod]
    [TestCategory("Integration")]
    [TestCategory("Day23")]
    public async Task CreateProduct_WithInvalidCategoryIds_ShouldFilterInvalidGuids()
    {
        // Arrange
        var validCategoryId = Guid.NewGuid();
        var request = new CreateProductRequest
        {
            Name = $"Product with invalid GUIDs - {Guid.NewGuid()}",
            Sku = $"TEST-SKU-{Guid.NewGuid():N}",
            ShortDescription = "Description",
            LongDescription = "Long description",
            Price = 100.00m,
            CategoryIds = new List<string>
            {
                validCategoryId.ToString(),
                "invalid-guid",
                "not-a-guid",
                "123"
            }
        };

        var content = CreateMultipartFormContent(request);

        // Act
        var response = await _httpClient!.PostAsync(ApiEndpoint, content);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode,
            "Invalid GUIDs should be filtered out");
    }

    #endregion

    #region Helper Methods

    private MultipartFormDataContent CreateMultipartFormContent(CreateProductRequest request)
    {
        var content = new MultipartFormDataContent();

        if (!string.IsNullOrEmpty(request.Name))
            content.Add(new StringContent(request.Name), "name");
        if (!string.IsNullOrEmpty(request.Sku))
            content.Add(new StringContent(request.Sku), "sku");
        if (!string.IsNullOrEmpty(request.ShortDescription))
            content.Add(new StringContent(request.ShortDescription), "shortDescription");
        if (!string.IsNullOrEmpty(request.LongDescription))
            content.Add(new StringContent(request.LongDescription), "longDescription");
        content.Add(new StringContent(request.Price.ToString()), "price");
        if (request.SalePrice.HasValue)
            content.Add(new StringContent(request.SalePrice.Value.ToString()), "salePrice");
        if (request.CategoryIds != null)
        {
            for (int i = 0; i < request.CategoryIds.Count; i++)
            {
                content.Add(new StringContent(request.CategoryIds[i]), $"categoryIds[{i}]");
            }
        }
        if (request.BrandId.HasValue)
            content.Add(new StringContent(request.BrandId.Value.ToString()), "brandId");
        if (request.Colors != null)
        {
            for (int i = 0; i < request.Colors.Count; i++)
            {
                content.Add(new StringContent(request.Colors[i]), $"colors[{i}]");
            }
        }
        if (request.Sizes != null)
        {
            for (int i = 0; i < request.Sizes.Count; i++)
            {
                content.Add(new StringContent(request.Sizes[i]), $"sizes[{i}]");
            }
        }
        if (request.Tags != null)
        {
            for (int i = 0; i < request.Tags.Count; i++)
            {
                content.Add(new StringContent(request.Tags[i]), $"tags[{i}]");
            }
        }
        content.Add(new StringContent(request.Published.ToString().ToLower()), "published");
        content.Add(new StringContent(request.Featured.ToString().ToLower()), "featured");
        if (!string.IsNullOrEmpty(request.SEOTittle))
            content.Add(new StringContent(request.SEOTittle), "seoTittle");
        if (!string.IsNullOrEmpty(request.SEODescription))
            content.Add(new StringContent(request.SEODescription), "seoDescription");
        if (!string.IsNullOrEmpty(request.Barcode))
            content.Add(new StringContent(request.Barcode), "barcode");
        if (!string.IsNullOrEmpty(request.Unit))
            content.Add(new StringContent(request.Unit), "unit");
        if (request.Weight.HasValue)
            content.Add(new StringContent(request.Weight.Value.ToString()), "weight");

        return content;
    }

    #endregion
}
