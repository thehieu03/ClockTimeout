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
        public const string GetProductById = $"{BaseAdmin}/{{producdtId}}";
        public const string GetAllProducts = $"{BaseAdmin}/all";
        public const string GetPublicProductById = $"{Base}/{{productId}}";
        public const string GetPublicProducts = $"{Base}";

        #endregion
    }
}