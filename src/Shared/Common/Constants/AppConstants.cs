namespace Common.Constants;

public sealed class AppConstants
{

    #region Common
    public const int MaxAttempts = 3;
    #endregion Bucket
    
    public const string Basket = "Basket";
    public static class Bucket
    {
        public const string Products= "products";
    }
    public static class Service
    {
        public const string Basket = "basket";
        public const string Catalog = "catalog";
        public const string Communication = "communication";
        public const string Discount = "discount";
        public const string Inventory = "inventory";
        public const string Notification = "notification";
        public const string Order = "order";
        public const string Report = "report";
        public const string Search = "search";
    }
    public static class FileContentType
    {
        public const string OctetStream = "application/octet-stream";
    }
}
