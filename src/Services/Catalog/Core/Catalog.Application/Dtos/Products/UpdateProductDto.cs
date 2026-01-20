using System.ComponentModel.DataAnnotations;
using Common.Models;

namespace Catalog.Application.Dtos.Products;

public class UpdateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 3)]
    public string? Name { get; set; }

    [Required(ErrorMessage = "SKU is required")]
    [StringLength(100, MinimumLength = 3)]
    public string? Sku { get; set; }

    [Required(ErrorMessage = "Short description is required")]
    [StringLength(500, MinimumLength = 10)]
    public string? ShortDescription { get; set; }

    [Required(ErrorMessage = "Long description is required")]
    [StringLength(5000, MinimumLength = 20)]
    public string? LongDescription { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999999999.99)]
    public decimal Price { get; set; }

    [Range(0.01, 999999999.99)]
    public decimal? SalePrice { get; set; }

    public List<Guid>? CategoryIds { get; set; }
    public List<UploadFileBytes>? UploadImages { get; set; }
    public UploadFileBytes? UploadThumbnail { get; set; }
    public Guid? BrandId { get; set; }
    public List<string>? Colors { get; set; }
    public List<string>? Sizes { get; set; }
    public List<string>? Tags { get; set; }
    public bool Published { get; set; }
    public bool Featured { get; set; }
    public string? SEOTitle { get; set; }
    public string? SEODescription { get; set; }
    public string? Barcode { get; set; }
    public string? Unit { get; set; }
    public decimal? Weight { get; set; }
}
