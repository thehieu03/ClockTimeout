using Catalog.Application.Services;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Services;
using Catalog.Infrastructure.Data;
using Marten;
using Marten.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TestProject1;

[TestClass]
public sealed class SeedDataServicesTests
{
    private Mock<IDocumentSession> _session = null!;
    private Mock<IMartenQueryable<CategoryEntity>> _categoryQuery = null!;
    private Mock<IMartenQueryable<BrandEntity>> _brandQuery = null!;
    private Mock<IMartenQueryable<ProductEntity>> _productQuery = null!;
    private SeedDataServices _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _session = new Mock<IDocumentSession>();
        _categoryQuery = new Mock<IMartenQueryable<CategoryEntity>>();
        _brandQuery = new Mock<IMartenQueryable<BrandEntity>>();
        _productQuery = new Mock<IMartenQueryable<ProductEntity>>();

        _service = new SeedDataServices();
    }

    [TestMethod]
    public async Task SeedDataAsync_WhenNoData_ShouldSeedAndSave()
    {
        var ct = CancellationToken.None;

        _categoryQuery.Setup(q => q.AnyAsync(ct)).ReturnsAsync(false);
        _brandQuery.Setup(q => q.AnyAsync(ct)).ReturnsAsync(false);
        _productQuery.Setup(q => q.CountAsync(ct)).ReturnsAsync(0);

        _session.Setup(s => s.Query<CategoryEntity>()).Returns(_categoryQuery.Object);
        _session.Setup(s => s.Query<BrandEntity>()).Returns(_brandQuery.Object);
        _session.Setup(s => s.Query<ProductEntity>()).Returns(_productQuery.Object);

        var result = await _service.SeedDataAsync(_session.Object, ct);

        Assert.IsTrue(result);
        _session.Verify(s => s.Store(It.IsAny<IEnumerable<CategoryEntity>>()), Times.AtLeastOnce);
        _session.Verify(s => s.Store(It.IsAny<IEnumerable<BrandEntity>>()), Times.AtLeastOnce);
        _session.Verify(s => s.Store(It.IsAny<IEnumerable<ProductEntity>>()), Times.AtLeastOnce);
        _session.Verify(s => s.SaveChangesAsync(ct), Times.Once);
    }

    [TestMethod]
    public async Task SeedDataAsync_WhenDataExists_ShouldNotSeed()
    {
        var ct = CancellationToken.None;

        _categoryQuery.Setup(q => q.AnyAsync(ct)).ReturnsAsync(true);
        _brandQuery.Setup(q => q.AnyAsync(ct)).ReturnsAsync(true);
        _productQuery.Setup(q => q.CountAsync(ct)).ReturnsAsync(ProductSeedData.TargetProductCount);

        _session.Setup(s => s.Query<CategoryEntity>()).Returns(_categoryQuery.Object);
        _session.Setup(s => s.Query<BrandEntity>()).Returns(_brandQuery.Object);
        _session.Setup(s => s.Query<ProductEntity>()).Returns(_productQuery.Object);

        var result = await _service.SeedDataAsync(_session.Object, ct);

        Assert.IsFalse(result);
        _session.Verify(s => s.Store(It.IsAny<IEnumerable<CategoryEntity>>()), Times.Never);
        _session.Verify(s => s.Store(It.IsAny<IEnumerable<BrandEntity>>()), Times.Never);
        _session.Verify(s => s.Store(It.IsAny<IEnumerable<ProductEntity>>()), Times.Never);
        _session.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
