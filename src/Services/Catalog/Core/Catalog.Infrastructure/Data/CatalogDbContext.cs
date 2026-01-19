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
    public DbSet<BrandEntity> Brands { get; set; } = null!;
    public DbSet<CategoryEntity> Categories { get; set; } = null!;
    public DbSet<OutboxMessageEntity> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureProductEntity(modelBuilder);
        ConfigureProductImageEntity(modelBuilder);
        ConfigureBrandEntity(modelBuilder);
        ConfigureCategoryEntity(modelBuilder);
        ConfigureOutboxMessageEntity(modelBuilder);
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
            entity.Property(e => e.Barcode)
                .HasColumnName("Barcode")
                .HasMaxLength(100)
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
            // Navigation properties - Ignored because ProductImageEntity is configured as standalone entity
            // ProductImageEntity is managed separately via DbSet<ProductImageEntity> ProductImages
            // If you need relationships, add ProductId foreign key to ProductImageEntity and configure relationships
            entity.Ignore(e => e.Images);
            entity.Ignore(e => e.Thumbnail);

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

    private void ConfigureBrandEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BrandEntity>(entity =>
        {
            entity.ToTable("Brands");
            // primary key
            entity.HasKey(b => b.Id);
            // properties configuration
            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .IsRequired();
            entity.Property(e => e.Name)
                .HasColumnName("Name")
                .HasMaxLength(200)
                .IsRequired(false);
            entity.Property(e => e.Slug)
                .HasColumnName("Slug")
                .HasMaxLength(200)
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
                .IsRequired(false);
            entity.Property(e => e.LastModifiedBy)
                .HasColumnName("LastModifiedBy")
                .HasMaxLength(200)
                .IsRequired(false);
            // Indexes
            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Brands_Slug");
            entity.HasIndex(e => e.Name)
                .HasDatabaseName("IX_Brands_Name");
        });
    }

    private void ConfigureCategoryEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            entity.ToTable("Categories");
            // primary key
            entity.HasKey(c => c.Id);
            // properties configuration
            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .IsRequired();
            entity.Property(e => e.Name)
                .HasColumnName("Name")
                .HasMaxLength(200)
                .IsRequired(false);
            entity.Property(e => e.Description)
                .HasColumnName("Description")
                .HasMaxLength(1000)
                .IsRequired(false);
            entity.Property(e => e.Slug)
                .HasColumnName("Slug")
                .HasMaxLength(200)
                .IsRequired(false);
            entity.Property(e => e.ParentId)
                .HasColumnName("ParentId")
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
                .IsRequired(false);
            entity.Property(e => e.LastModifiedBy)
                .HasColumnName("LastModifiedBy")
                .HasMaxLength(200)
                .IsRequired(false);
            // Self-referencing relationship (no navigation property, using HasForeignKey directly)
            entity.HasOne<CategoryEntity>()
                .WithMany()
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            // Indexes
            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("IX_Categories_Slug");
            entity.HasIndex(e => e.Name)
                .HasDatabaseName("IX_Categories_Name");
            entity.HasIndex(e => e.ParentId)
                .HasDatabaseName("IX_Categories_ParentId");
        });
    }

    private void ConfigureOutboxMessageEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessageEntity>(entity =>
        {
            entity.ToTable("OutboxMessages");
            // primary key
            entity.HasKey(o => o.Id);
            // properties configuration
            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .IsRequired();
            entity.Property(e => e.EventType)
                .HasColumnName("EventType")
                .HasMaxLength(200)
                .IsRequired(false);
            entity.Property(e => e.Content)
                .HasColumnName("Content")
                .HasColumnType("text")
                .IsRequired(false);
            entity.Property(e => e.OccurredOnUtc)
                .HasColumnName("OccurredOnUtc")
                .IsRequired();
            entity.Property(e => e.ProcessedOnUtc)
                .HasColumnName("ProcessedOnUtc")
                .IsRequired(false);
            entity.Property(e => e.LastErrorMessage)
                .HasColumnName("LastErrorMessage")
                .HasMaxLength(2000)
                .IsRequired(false);
            entity.Property(e => e.ClaimedOnUtc)
                .HasColumnName("ClaimedOnUtc")
                .IsRequired(false);
            entity.Property(e => e.AttemptCount)
                .HasColumnName("AttemptCount")
                .IsRequired();
            entity.Property(e => e.MaxAttemptCount)
                .HasColumnName("MaxAttemptCount")
                .IsRequired();
            entity.Property(e => e.NextAttemptOnUtc)
                .HasColumnName("NextAttemptOnUtc")
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
                .IsRequired(false);
            entity.Property(e => e.LastModifiedBy)
                .HasColumnName("LastModifiedBy")
                .HasMaxLength(200)
                .IsRequired(false);
            // Indexes
            entity.HasIndex(e => e.EventType)
                .HasDatabaseName("IX_OutboxMessages_EventType");
            entity.HasIndex(e => e.OccurredOnUtc)
                .HasDatabaseName("IX_OutboxMessages_OccurredOnUtc");
            entity.HasIndex(e => e.ProcessedOnUtc)
                .HasDatabaseName("IX_OutboxMessages_ProcessedOnUtc");
            entity.HasIndex(e => new { e.ProcessedOnUtc, e.NextAttemptOnUtc })
                .HasDatabaseName("IX_OutboxMessages_Processed_NextAttempt");
        });
    }
}
