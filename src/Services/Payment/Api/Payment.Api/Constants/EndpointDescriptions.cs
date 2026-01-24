namespace Payment.Api.Constants;

public static class EndpointDescriptions
{
    public static class Payment
    {
        public const string Create = "Create a new payment for an order";
        public const string GetAll = "Get all payments";
        public const string GetById = "Get payment by ID";
        public const string GetByOrderId = "Get payment by order ID";
        public const string GetByStatus = "Get payments by status";
        public const string Complete = "Mark payment as completed";
        public const string Fail = "Mark payment as failed";
        public const string Refund = "Refund a completed payment";
        public const string Process = "Process a pending payment through the payment gateway";
        public const string VnPayCallback = "VNPay callback URL - redirects user after payment";
        public const string VnPayIpn = "VNPay IPN URL - receives payment notifications from VNPay";
    }
}
