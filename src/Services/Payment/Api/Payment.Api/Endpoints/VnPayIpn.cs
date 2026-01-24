using Carter;
using MediatR;
using Microsoft.Extensions.Options;
using Payment.Api.Constants;
using Payment.Application.Features.Payment.Commands;
using Payment.Application.Models.Results;
using Payment.Infrastructure.Configurations;
using Payment.Infrastructure.Gateways.VnPay;

namespace Payment.Api.Endpoints;

public sealed class VnPayIpn : ICarterModule
{
    #region Implementations

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Payment.VnPayIpn, HandleVnPayIpnAsync)
            .WithTags("VNPay")
            .WithName(nameof(VnPayIpn))
            .Produces<VnPayIpnResponse>(StatusCodes.Status200OK)
            .WithDescription(EndpointDescriptions.Payment.VnPayIpn)
            .AllowAnonymous();
    }

    #endregion

    #region Methods

    private async Task<IResult> HandleVnPayIpnAsync(
        HttpContext context,
        ISender sender,
        IOptions<VnPaySettings> settings,
        ILogger<VnPayIpn> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            // Parse query string from VnPay
            var queryString = context.Request.QueryString.Value ?? "";
            var vnpParams = VnPayHelper.ParseQueryString(queryString);

            logger.LogInformation("[VNPay IPN] Received IPN: {Query}", queryString);

            // Check required parameters
            if (!vnpParams.ContainsKey("vnp_TxnRef") || !vnpParams.ContainsKey("vnp_SecureHash"))
            {
                logger.LogWarning("[VNPay IPN] Missing required parameters");
                return Results.Ok(new VnPayIpnResponse("99", "Missing required parameters"));
            }

            // Get secure hash from params
            var secureHash = vnpParams.GetValueOrDefault("vnp_SecureHash", "");

            // Validate signature
            var isValidSignature = VnPayHelper.ValidateSignature(
                vnpParams,
                secureHash,
                settings.Value.HashSecret);

            if (!isValidSignature)
            {
                logger.LogWarning("[VNPay IPN] Invalid signature");
                return Results.Ok(new VnPayIpnResponse("97", "Invalid signature"));
            }

            // Parse result
            var callbackResult = VnPayCallbackResult.FromVnPayResponse(vnpParams, true);
            var txnRef = callbackResult.TransactionId;

            // Process the IPN via command handler
            var command = new HandleVnPayCallbackCommand(callbackResult);
            var result = await sender.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("[VNPay IPN] IPN processed successfully for TxnRef: {TxnRef}", txnRef);
                return Results.Ok(new VnPayIpnResponse("00", "Confirm Success"));
            }
            else
            {
                // Check if payment not found
                if (result.Message?.Contains("not found") == true)
                {
                    logger.LogWarning("[VNPay IPN] Payment not found for TxnRef: {TxnRef}", txnRef);
                    return Results.Ok(new VnPayIpnResponse("01", "Order not found"));
                }

                logger.LogWarning("[VNPay IPN] IPN processing failed for TxnRef: {TxnRef}, Message: {Message}", txnRef, result.Message);
                return Results.Ok(new VnPayIpnResponse("99", result.Message ?? "Unknown error"));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[VNPay IPN] Error processing IPN");
            return Results.Ok(new VnPayIpnResponse("99", "Error processing IPN"));
        }
    }

    #endregion
}