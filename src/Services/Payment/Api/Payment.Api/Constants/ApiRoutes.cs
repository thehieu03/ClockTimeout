namespace Payment.Api.Constants;

public static class ApiRoutes
{
    public static class Payment
    {
        public const string Tags = "Payments";
        private const string Base = "/payments";
        private const string BaseAdmin = "/admin/payments";

        public const string Create = $"{BaseAdmin}";
        public const string GetAll = $"{BaseAdmin}";
        public const string GetById = $"{BaseAdmin}/{{paymentId}}";
        public const string GetByOrderId = $"{Base}/by-order/{{orderId}}";
        public const string GetByStatus = $"{BaseAdmin}/by-status/{{status}}";
        public const string Complete = $"{BaseAdmin}/{{paymentId}}/complete";
        public const string Fail = $"{BaseAdmin}/{{paymentId}}/fail";
        public const string Refund = $"{BaseAdmin}/{{paymentId}}/refund";
    }
}
