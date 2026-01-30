using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Payment.Infrastructure.Gateways.VnPay;

public static class VnPayHelper
{
    public static string BuildPaymentUrl(
        string baseUrl,
        string tmnCode,
        string hashSecret,
        string txnRef,
        decimal amount,
        string orderInfo,
        string returnUrl,
        string ipAddress,
        string locale = "vn",
        string currencyCode = "VND",
        string version = "2.1.0",
        string? bankCode = null
        )
    {
        var vnpParams = new SortedDictionary<string, string>()
        {
            {
                "vnp_Version", version
            },
            {
                "vnp_Command", "pay"
            },
            {
                "vnp_TmnCode", tmnCode
            },
            {
                "vnp_Amount", ((long)(amount * 100)).ToString()
            },
            {
                "vnp_CurrCode", currencyCode
            },
            {
                "vnp_TxnRef", txnRef
            },
            {
                "vnp_OrderInfo", orderInfo
            },
            {
                "vnp_OrderType", "other"
            },
            {
                "vnp_Locale", locale
            },
            {
                "vnp_ReturnUrl", returnUrl
            },
            {
                "vnp_IpAddr", ipAddress
            },
            {
                "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")
            },
            {
                "vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss")
            }
        };
        if (!string.IsNullOrEmpty(bankCode))
        {
            vnpParams.Add("vnp_BankCode", bankCode);
        }
        var queryString = BuildQueryString(vnpParams);
        var signData = queryString;
        var signature = ComputeHmacSha512(hashSecret, signData);
        return $"{baseUrl}?{queryString}&vnp_SecureHash={signature}";
    }
    public static bool ValidateSignature(
        IDictionary<string, string> vnpParams,
        string inputHash,
        string hashSecret)
    {
        var validationParams = new SortedDictionary<string, string>(
            vnpParams.Where(x => 
                !x.Key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                !x.Key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Key, x => x.Value));

        var signData = BuildQueryString(validationParams);
        var checkSum = ComputeHmacSha512(hashSecret, signData);

        return checkSum.Equals(inputHash, StringComparison.OrdinalIgnoreCase);
    }
    public static Dictionary<string, string> ParseQueryString(string queryString)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(queryString))
            return result;

        // Remove leading '?' if present
        if (queryString.StartsWith("?"))
            queryString = queryString.Substring(1);

        var pairs = queryString.Split('&');
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=');
            if (keyValue.Length == 2)
            {
                var key = WebUtility.UrlDecode(keyValue[0]);
                var value = WebUtility.UrlDecode(keyValue[1]);
                result[key] = value;
            }
        }

        return result;
    }
    public static string GetResponseMessage(string responseCode)
    {
        return responseCode switch
        {
            "00" => "Giao dịch thành công",
            "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường)",
            "09" => "Thẻ/Tài khoản chưa đăng ký dịch vụ InternetBanking",
            "10" => "Xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
            "11" => "Đã hết hạn chờ thanh toán",
            "12" => "Thẻ/Tài khoản bị khóa",
            "13" => "Nhập sai mật khẩu xác thực giao dịch (OTP)",
            "24" => "Khách hàng hủy giao dịch",
            "51" => "Tài khoản không đủ số dư",
            "65" => "Tài khoản đã vượt quá hạn mức giao dịch trong ngày",
            "75" => "Ngân hàng thanh toán đang bảo trì",
            "79" => "Nhập sai mật khẩu thanh toán quá số lần quy định",
            "99" => "Lỗi không xác định",
            _ => $"Lỗi không xác định: {responseCode}"
        };
    }
    public static bool IsSuccessTransaction(string responseCode, string transactionStatus)
    {
        return responseCode == "00" && transactionStatus == "00";
    }

    private static string BuildQueryString(SortedDictionary<string, string> parameters)
    {
        var sb = new StringBuilder();
        foreach (var kvp in parameters)
        {
            if (!string.IsNullOrEmpty(kvp.Value))
            {
                if (sb.Length > 0)
                    sb.Append('&');
                sb.Append(WebUtility.UrlEncode(kvp.Key));
                sb.Append('=');
                sb.Append(WebUtility.UrlEncode(kvp.Value));
            }
        }
        return sb.ToString();
    }

    public static string ComputeHmacSha512(string key, string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}