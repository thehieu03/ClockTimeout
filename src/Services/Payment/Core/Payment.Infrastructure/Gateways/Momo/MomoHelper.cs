using System.Security.Cryptography;

namespace Payment.Infrastructure.Gateways.Momo;

public static class MomoHelper
{
    public static string GenerateSignature(string rawData, string secretKey)
    {
        using var hmac = new HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secretKey));
        var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
    public static string BuildCreatePaymentRawData(
        string accessKey,
        decimal amount,
        string extraData,
        string ipnUrl,
        string orderId,
        string orderInfor,
        string partnerCode,
        string redirectUrl,
        string requestId,
        string requestType)
    {
        if (!string.IsNullOrEmpty(accessKey) &&
            !string.IsNullOrEmpty(amount.ToString()) &&
            !string.IsNullOrEmpty(extraData) &&
            !string.IsNullOrEmpty(ipnUrl) &&
            !string.IsNullOrEmpty(orderId) &&
            !string.IsNullOrEmpty(orderInfor) &&
            !string.IsNullOrEmpty(partnerCode) &&
            !string.IsNullOrEmpty(redirectUrl) &&
            !string.IsNullOrEmpty(requestId) &&
            !string.IsNullOrEmpty(requestType))
        {
            return $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}" +
                $"&orderId={orderId}&orderInfo={orderInfor}&partnerCode={partnerCode}&redirectUrl={redirectUrl}" +
                $"&requestId={requestId}&requestType={requestType}";
        }
        return string.Empty;
    }
    public static bool VerifySignature(string rawData, string secretKey, string signature)
    {
        var generatedSignature = GenerateSignature(rawData, secretKey);
        return generatedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
    }
    public static string GetResultMessage(int resultCode)
    {
        return resultCode switch
        {
            0 => "Giao dịch thành công",
            9000 => "Giao dịch đang được xử lý",
            8000 => "Giao dịch đang được tương tác bởi đối tác Momo",
            3 => "Hệ thống đang bảo trì",
            4 => "Truyền thiếu sai thông tin bắt buộc",
            5 => "Đơn hàng không hợp lệ",
            6 => "Số tiền không hợp lệ",
            7 => "Đơn hàng đã tồn tại",
            8 => "Đối tác không hợp lệ",
            9 => "Phương thức thanh toán không hợp lệ",
            10 => "Mã bảo mật không hợp lệ",
            11 => "Số tiền vượt quá hạn mức",
            12 => "Đơn hàng đã được thanh toán",
            13 => "Quá thời gian cho phép thanh toán đơn hàng",
            14 => "Số tiền nhỏ hơn hạn mức thanh toán tối thiểu",
            15 => "Tài khoản người dùng không tồn tại",
            16 => "Tài khoản người dùng bị khóa",
            17 => "Người dùng hủy giao dịch",
            18 => "Xác thực giao dịch thất bại",
            19 => "Giao dịch thất bại",
            20 => "Giao dịch bị từ chối",
            21 => "Số lần thử vượt quá giới hạn",
            22 => "Phiên giao dịch không hợp lệ",
            23 => "Tài nguyên không tồn tại",
            24 => "Tài nguyên đã tồn tại",
            _ => "Unrecognized result code"
        };
    }
}
