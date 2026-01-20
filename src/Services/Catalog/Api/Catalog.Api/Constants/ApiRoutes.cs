namespace Catalog.Api.Constants;

public sealed class ApiRoutes
{
    public static class Product
    {
        #region Constants
        public const string Tags = "Products";
        public const string Base = "/products";
        public const string BaseAdmin = "/admin/products";
        public const string Create = $"{BaseAdmin}";
        public const string Delete = $"{BaseAdmin}/{{productId}}";
        public const string Update = $"{BaseAdmin}/{{productId}}";
        public const string Unpublish = $"{BaseAdmin}/{{productId}}/unpublish";
        public const string Publish = $"{BaseAdmin}/{{productId}}/publish";
        public const string GetProducts = $"{BaseAdmin}";
        public const string GetProductById = $"{BaseAdmin}/{{productId}}";
        public const string GetAllProducts = $"{BaseAdmin}/all";
        public const string GetPublicProductById = $"{Base}/{{productId}}";
        public const string GetPublicProducts = $"{Base}";

        #endregion
    }

    public static class Category
    {
        #region Constants
        public const string Tags = "Categories";
        public const string Base = "/categories";
        public const string BaseAdmin = "/admin/categories";
        public const string Create = $"{BaseAdmin}";
        public const string Update = $"{BaseAdmin}/{{categoryId}}";
        public const string Delete = $"{BaseAdmin}/{{categoryId}}";
        public const string GetAll = $"{BaseAdmin}";
        public const string GetTree = $"{BaseAdmin}/tree";
        #endregion
    }

    public static class Brand
    {
        #region Constants
        public const string Tags = "Brands";
        public const string Base = "/brands";
        public const string BaseAdmin = "/admin/brands";
        public const string Create = $"{BaseAdmin}";
        public const string Update = $"{BaseAdmin}/{{brandId}}";
        public const string Delete = $"{BaseAdmin}/{{brandId}}";
        public const string GetAll = $"{BaseAdmin}";
        #endregion
    }
}