namespace Order.Api.Constants;

public sealed class ApiRoutes
{
    public static class Order
    {
        public const string Tags = "Orders";
        private const string Base = "/orders";
        private const string BaseAdmin = "/admin/orders";
        public const string Create = $"{BaseAdmin}";
        public const string GetById = $"{BaseAdmin}/{{orderId}}";
        public const string GetOrderByOrderNo = $"{Base}/by-order-no/{{orderNo}}";
        public const string GetOrdersByCurrentUser = $"{Base}/me";
        public const string UpdateStatus = $"{BaseAdmin}/{{orderId}}/status";
    }
}