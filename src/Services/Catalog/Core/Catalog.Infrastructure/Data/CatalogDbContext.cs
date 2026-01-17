using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProductEntity> Products { get; set; } = null!;
    public DbSet<ProductImageEntity> ProductImages { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
    private void ConfigureProductEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("Products");
            // primary key
            entity.HasKey(p => p.Id);
            // properties configuration
            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .IsRequired();
            entity.Property(e => e.Name)
                .HasColumnName("Name")
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(e => e.Sku)
                .HasColumnName("Sku")
                .HasMaxLength(100)
                .IsRequired(false);
            entity.Property(e => e.ShortDescription)
                .HasColumnName("ShortDescription")
                .HasMaxLength(500)
                .IsRequired(false);
            entity.Property(e => e.LongDescription)
                .HasColumnName("LongDescription")
                .HasMaxLength(5000)
                .IsRequired(false);
            entity.Property(e => e.Slug)
                .HasColumnName("Slug")
                .HasMaxLength(200)
                .IsRequired(false);
            entity.Property(e => e.Price)
                .HasColumnName("Price")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            entity.Property(e => e.SalePrice)
                .HasColumnName("SalePrice")
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);
            entity.Property(e => e.Published)
                .HasColumnName("Published")
                .IsRequired();
            entity.Property(e => e.Featured)
                .HasColumnName("Featured")
                .IsRequired();
            entity.Property(e => e.Status)
                .HasColumnName("Status")
                .IsRequired();
            entity.Property(e => e.BrandId)
                .HasColumnName("BrandId")
                .IsRequired(false);
            entity.Property(e => e.SEOTitle)
                .HasColumnName("SEOTitle")
                .HasMaxLength(200)
                .IsRequired(false);
            entity.Property(e => e.SEODescription)
                .HasColumnName("SEODescription")
                .HasMaxLength(500)
                .IsRequired(false);
            entity.Property(e => e.Unit)
                .HasColumnName("Unit")
                .HasMaxLength(50)
                .IsRequired(false);
            entity.Property(e => e.Weight)
                .HasColumnName("Weight")
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);
            entity.Property(e => e.CreatedOnUtc)
                .HasColumnName("CreatedOnUtc")
                .IsRequired();
            entity.Property(e => e.CreatedBy)
                .HasColumnName("CreatedBy")
                .HasMaxLength(200)
                .IsRequired(false);
            entity.Property(e => e.LastModifiedOnUtc)
                .HasColumnName("LastModifiedOnUtc")
                .IsRequired();
            entity.Property(e => e.LastModifiedBy)
                .HasColumnName("LastModifiedBy")
                .HasMaxLength(200)
                .IsRequired(false);
            // Collections - stored as JSON in PostgreSQL
            entity.Property(e => e.CategoryIds)
                .HasColumnName("CategoryIds")
                .HasColumnType("jsonb")
                .IsRequired(false);
            entity.Property(e => e.Colors)

                .HasColumnName("Colors")
                .HasColumnType("jsonb")
                .IsRequired(false);
            entity.Property(e => e.Sizes)
                .HasColumnName("Sizes")
                .HasColumnType("jsonb")
                .IsRequired(false);
            entity.Property(e => e.Tags)
                .HasColumnName("Tags")
                .HasColumnType("jsonb")
                .IsRequired(false);
            // Navigation properties
        });
    }
}
