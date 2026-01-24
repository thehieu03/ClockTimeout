using Carter;
using MediatR;
using Microsoft.Extensions.Options;
using Payment.Api.Constants;
using Payment.Application.Features.Payment.Commands;
using Payment.Application.Models.Results;
using Payment.Infrastructure.Configurations;
using Payment.Infrastructure.Gateways.VnPay;

namespace Payment.Api.Endpoints;

public sealed class VnPayCallback : ICarterModule
{
    #region Implementations

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Payment.VnPayCallback, HandleVnPayCallbackAsync)
            .WithTags("VNPay")
            .WithName(nameof(VnPayCallback))
            .Produces(StatusCodes.Status302Found)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription(EndpointDescriptions.Payment.VnPayCallback)
            .AllowAnonymous();
    }

    #endregion

    #region Methods

    private async Task<IResult> HandleVnPayCallbackAsync(
        HttpContext httpContext,
        ISender sender,
        IOptions<VnPaySettings> vnpaySettings,
        IConfiguration configuration,
        ILogger<VnPayCallback> logger,
        CancellationToken cancellationToken)
    {
        // Get frontend callback URL from configuration
        var frontendBaseUrl = configuration["FrontendUrl"] ?? "http://localhost:3000";
        var successUrl = $"{frontendBaseUrl}/payment/success";
        var failureUrl = $"{frontendBaseUrl}/payment/failure";

        try
        {
            // Parse query string
            var queryString = httpContext.Request.QueryString.Value ?? "";
            var vnpParams = VnPayHelper.ParseQueryString(queryString);

            logger.LogInformation("[VNPay Callback] Received callback: {Query}", queryString);

            // Get secure hash from params
            var secureHash = vnpParams.GetValueOrDefault("vnp_SecureHash", "");

            // Validate signature
            var isValidSignature = VnPayHelper.ValidateSignature(
                vnpParams,
                secureHash,
                vnpaySettings.Value.HashSecret);

            if (!isValidSignature)
            {
                logger.LogWarning("[VNPay Callback] Invalid signature");
                return Results.Redirect($"{failureUrl}?error=invalid_signature");
            }

            // Parse result
            var callbackResult = VnPayCallbackResult.FromVnPayResponse(vnpParams, true);
            var txnRef = callbackResult.TransactionId;
            var responseCode = callbackResult.ResponseCode ?? "99";
            var responseMessage = VnPayHelper.GetResponseMessage(responseCode);

            // Process the callback
            var command = new HandleVnPayCallbackCommand(callbackResult);
            var result = await sender.Send(command, cancellationToken);

            if (result.IsSuccess && callbackResult.IsSuccess)
            {
                logger.LogInformation("[VNPay Callback] Payment successful for TxnRef: {TxnRef}", txnRef);
                
                // Redirect to success page with payment info
                var redirectUrl = $"{successUrl}?txnRef={txnRef}&paymentId={result.PaymentId}&amount={callbackResult.Amount}";
                return Results.Redirect(redirectUrl);
            }
            else
            {
                logger.LogWarning("[VNPay Callback] Payment failed for TxnRef: {TxnRef}, Code: {Code}", txnRef, responseCode);
                
                // Redirect to failure page with error info
                var redirectUrl = $"{failureUrl}?txnRef={txnRef}&code={responseCode}&message={Uri.EscapeDataString(responseMessage)}";
                return Results.Redirect(redirectUrl);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[VNPay Callback] Error processing callback");
            return Results.Redirect($"{failureUrl}?error=processing_error");
        }
    }

    #endregion
}

public record VnPayIpnResponse(string RspCode, string Message);