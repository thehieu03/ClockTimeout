using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Payment.Api.Constants;
using Payment.Application.Features.Payment.Commands;
using Payment.Infrastructure.Configurations;
using Payment.Infrastructure.Gateways.Momo;
using Payment.Infrastructure.Gateways.Momo.Models;
using Common.ValueObjects;

namespace Payment.Api.Endpoints;

public class MomoEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments/momo").WithTags("Momo");

        group.MapPost("/ipn", HandleIpn);
        group.MapGet("/return", HandleReturn);
    }

    // IPN: Server to Server (Quan trọng nhất)
    private async Task<IResult> HandleIpn(
        [FromBody] MomoIpnRequest request,
        ISender sender,
        IOptions<MomoSettings> options,
        ILogger<MomoEndpoints> logger)
    {
        logger.LogInformation("Received Momo IPN for OrderId: {OrderId}, ResultCode: {ResultCode}", request.OrderId, request.ResultCode);

        // 1. Verify Signature
        // Format signature IPN của Momo khác với CreateRequest
        // accessKey=$accessKey&amount=$amount&extraData=$extraData&message=$message&orderId=$orderId&orderInfo=$orderInfo&orderType=$orderType&partnerCode=$partnerCode&payType=$payType&requestId=$requestId&responseTime=$responseTime&resultCode=$resultCode&transId=$transId

        var rawSignature = $"accessKey={options.Value.AccessKey}&amount={request.Amount}&extraData={request.ExtraData}&message={request.Message}&orderId={request.OrderId}&orderInfo={request.OrderInfo}&orderType={request.OrderType}&partnerCode={request.PartnerCode}&payType={request.PayType}&requestId={request.RequestId}&responseTime={request.ResponseTime}&resultCode={request.ResultCode}&transId={request.TransId}";

        var signature = MomoHelper.ComputeHmacSha256(rawSignature, options.Value.SecretKey);

        if (signature != request.Signature)
        {
            logger.LogError("Invalid Signature in Momo IPN. Expected: {Exp}, Got: {Got}", signature, request.Signature);
            return Results.BadRequest(new { message = "Invalid Signature" });
        }

        // 2. Process Order
        var isSuccess = request.ResultCode == 0;
        var paymentId = Guid.Parse(request.OrderId); // Quy ước OrderId của Momo là PaymentId

        var command = new HandlePaymentCallbackCommand(
            PaymentId: paymentId,
            IsSuccess: isSuccess,
            TransactionId: request.TransId.ToString(),
            ResultCode: request.ResultCode.ToString(),
            ResultMessage: request.Message,
            RawResponse: System.Text.Json.JsonSerializer.Serialize(request),
            Gateway: "Momo",
            Actor: Actor.System("momo-webhook")
        );

        await sender.Send(command);

        // 3. Response to Momo (204 No Content is OK)
        return Results.NoContent();
    }

    // Return: User redirect back (Chỉ hiển thị UI kết quả)
    private async Task<IResult> HandleReturn(
        [AsParameters] MomoIpnRequest request, // Momo return GET params tương tự IPN model
        ISender sender,
        IOptions<MomoSettings> options)
    {
        // Tương tự IPN, nhưng đây là GET request.
        // Thực tế ReturnUrl chỉ nên dùng để check status và hiển thị "Thành công/Thất bại" cho user.
        // Logic update DB nên tin tưởng vào IPN hơn.

        // Demo đơn giản: trả về text info
        return Results.Ok(new {
            Message = "Payment Processed",
            Status = request.ResultCode == 0 ? "Success" : "Failed",
            OrderId = request.OrderId
        });
    }
}
