using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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
            9000 => "Giao dịch đã được xác nhận thành công",
            8000 => "Giao dịch đang được xử lý",
            7000 => "Giao dịch đang được xử lý bởi đối tác MoMo",
            1000 => "Hệ thống đang bảo trì",
            1001 => "Truyền thiếu/sai thông tin bắt buộc",
            1002 => "Đơn hàng không hợp lệ",
            1003 => "Đơn hàng đã tồn tại",
            1004 => "Số tiền không hợp lệ",
            1005 => "Chữ ký không hợp lệ",
            1006 => "Đơn hàng đã được thanh toán/hủy",
            1007 => "Tài khoản không đủ số dư",
            1017 => "Giao dịch bị từ chối bởi người dùng",
            1026 => "Giao dịch bị hạn chế theo quy định",
            1080 => "Giao dịch hoàn tiền không hợp lệ",
            1081 => "Giao dịch hoàn tiền đã tồn tại",
            2001 => "Giao dịch thất bại",
            2007 => "Giao dịch bị từ chối vì lý do bảo mật",
            3001 => "Đối tác chưa được liên kết",
            3002 => "Đối tác chưa được kích hoạt",
            3003 => "Đối tác đang bị tạm khóa",
            3004 => "Đối tác đã bị vô hiệu hóa",
            4001 => "Giao dịch bị hạn chế",
            4010 => "Người dùng chưa đăng ký dịch vụ",
            4011 => "Người dùng chưa liên kết tài khoản ngân hàng",
            4015 => "Người dùng đã hủy giao dịch",
            4100 => "Người dùng chưa xác thực tài khoản",
            _ => $"Lỗi không xác định: {resultCode}"
        };
    }
    public static bool IsSuccessTransaction(int resultCode)
    {
        return resultCode == 0 || resultCode == 9000;
    }
    public static string GenerateRequestId()
    {
        return $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}".Substring(0, 50);
    }
    /// <summary>
    /// Encode extra data to Base64
    /// </summary>
    public static string EncodeExtraData(Dictionary<string, string> data)
    {
        if (data == null || data.Count == 0)
            return string.Empty;

        var json = JsonSerializer.Serialize(data);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }
    public static Dictionary<string, string> DecodeExtraData(string encodedData)
    {
        if (string.IsNullOrEmpty(encodedData))
            return new Dictionary<string, string>();

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(encodedData));
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
    public static string ComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        byte[] hashBytes;

        using (var hmac = new HMACSHA256(keyBytes))
        {
            hashBytes = hmac.ComputeHash(messageBytes);
        }

        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        return hashString;
    }

    /// <summary>
    /// sort theo alphabet key và nối chuỗi key=value&...
    /// Momo quy định thứ tự params cụ thể cho từng API, nên dùng hàm build riêng an toàn hơn.
    /// </summary>
    public static string BuildRawSignature(string accessKey, string amount, string extraData, string ipnUrl,
        string orderId, string orderInfo, string partnerCode, string redirectUrl, string requestId, string requestType)
    {
        // Format chuẩn của Momo Create Payment:
        // accessKey=$accessKey&amount=$amount&extraData=$extraData&ipnUrl=$ipnUrl&orderId=$orderId&orderInfo=$orderInfo&partnerCode=$partnerCode&redirectUrl=$redirectUrl&requestId=$requestId&requestType=$requestType

        return $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";
    }
}
