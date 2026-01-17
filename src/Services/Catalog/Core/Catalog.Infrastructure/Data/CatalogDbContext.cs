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
        ConfigureProductEntity(modelBuilder);
        ConfigureProductImageEntity(modelBuilder);
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
            entity.OwnsMany(e => e.Images, img =>
            {
                img.ToTable("ProductImages");
                img.WithOwner().HasForeignKey("ProductId");
                img.Property(i => i.FileId).HasMaxLength(200);
                img.Property(i => i.OriginalFileName).HasMaxLength(500);
                img.Property(i => i.FileName).HasMaxLength(500);
                img.Property(i => i.PublicURL).HasMaxLength(1000);
            });

            entity.OwnsOne(e => e.Thumbnail, thumb =>
            {
                thumb.ToTable("ProductThumbnails");
                thumb.WithOwner().HasForeignKey("ProductId");
                thumb.Property(t => t.FileId).HasMaxLength(200);
                thumb.Property(t => t.OriginalFileName).HasMaxLength(500);
                thumb.Property(t => t.FileName).HasMaxLength(500);
                thumb.Property(t => t.PublicURL).HasMaxLength(1000);
            });

            // Indexes
            entity.HasIndex(e => e.Sku)
                .IsUnique()
                .HasDatabaseName("IX_Products_Sku");

            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Products_Slug");

            entity.HasIndex(e => e.Name)
                .HasDatabaseName("IX_Products_Name");

            entity.HasIndex(e => e.BrandId)
                .HasDatabaseName("IX_Products_BrandId");

            entity.HasIndex(e => e.Published)
                .HasDatabaseName("IX_Products_Published");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Products_Status");
        });
    }

    private void ConfigureProductImageEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductImageEntity>(entity =>
        {
            entity.ToTable("ProductImages");
            // primary key
            entity.HasKey(p => p.FileId);
            // properties configuration
            entity.Property(e => e.FileId)
                .HasColumnName("FileId")
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(e => e.OriginalFileName)
                .HasColumnName("OriginalFileName")
                .HasMaxLength(500)
                .IsRequired(false);
            entity.Property(e => e.FileName)
                .HasColumnName("FileName")
                .HasMaxLength(500)
                .IsRequired(false);
            entity.Property(e => e.PublicURL)
                .HasColumnName("PublicURL")
                .HasMaxLength(1000)
                .IsRequired(false);
            // Indexes
            entity.HasIndex(e => e.FileId)
                .IsUnique()
                .HasDatabaseName("IX_ProductImages_FileId");
        });
    }
}
