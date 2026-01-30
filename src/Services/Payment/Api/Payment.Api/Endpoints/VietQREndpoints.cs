using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Payment.Infrastructure.Configurations;

namespace Payment.Api.Endpoints;

/// <summary>
/// VietQR Payment Gateway Endpoints
/// </summary>
public class VietQREndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments/vietqr")
            .WithTags("VietQR")
            .WithOpenApi();

        group.MapGet("/generate", HandleGenerateQRAsync)
            .WithName("GenerateVietQR")
            .WithSummary("Generate VietQR payment QR code")
            .WithDescription("Generates a VietQR payment QR code image URL for the specified amount and description")
            .Produces<VietQRResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/info", HandleGetInfoAsync)
            .WithName("GetVietQRInfo")
            .WithSummary("Get VietQR account information")
            .WithDescription("Returns the configured bank account information for VietQR payments")
            .Produces<VietQRAccountInfo>(StatusCodes.Status200OK);
    }

    /// <summary>
    /// Generate VietQR payment QR code
    /// </summary>
    private Task<IResult> HandleGenerateQRAsync(
        [FromQuery] decimal amount,
        [FromQuery] string? description,
        [FromQuery] string? orderId,
        IOptions<VietQRSettings> settings,
        ILogger<VietQREndpoints> logger)
    {
        var config = settings.Value;

        if (amount <= 0)
        {
            logger.LogWarning("Invalid amount for VietQR generation: {Amount}", amount);
            return Task.FromResult(Results.BadRequest(new { message = "Amount must be greater than 0" }));
        }

        // Build description with orderId if provided
        var paymentDescription = string.IsNullOrEmpty(orderId)
            ? description ?? "Thanh toan don hang"
            : $"Thanh toan don hang {orderId}";

        // VietQR URL format: {ApiUrl}{BankBin}-{AccountNo}-{TemplateId}.png?amount={amount}&addInfo={description}&accountName={accountName}
        var qrUrl = $"{config.ApiUrl}{config.BankBin}-{config.AccountNo}-{config.TemplateId}.png" +
                    $"?amount={amount:F0}" +
                    $"&addInfo={Uri.EscapeDataString(paymentDescription)}" +
                    $"&accountName={Uri.EscapeDataString(config.AccountName)}";

        logger.LogInformation(
            "Generated VietQR URL for amount {Amount}, description: {Description}",
            amount, paymentDescription);

        var response = new VietQRResponse
        {
            QRCodeUrl = qrUrl,
            Amount = amount,
            Description = paymentDescription,
            AccountNo = config.AccountNo,
            AccountName = config.AccountName,
            BankBin = config.BankBin,
            OrderId = orderId
        };

        return Task.FromResult(Results.Ok(response));
    }

    /// <summary>
    /// Get VietQR account information
    /// </summary>
    private Task<IResult> HandleGetInfoAsync(
        IOptions<VietQRSettings> settings,
        ILogger<VietQREndpoints> logger)
    {
        var config = settings.Value;

        logger.LogInformation("Returning VietQR account information");

        var info = new VietQRAccountInfo
        {
            BankBin = config.BankBin,
            AccountNo = config.AccountNo,
            AccountName = config.AccountName,
            ApiUrl = config.ApiUrl
        };

        return Task.FromResult(Results.Ok(info));
    }
}

/// <summary>
/// VietQR generation response
/// </summary>
public record VietQRResponse
{
    public string QRCodeUrl { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Description { get; init; } = string.Empty;
    public string AccountNo { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public string BankBin { get; init; } = string.Empty;
    public string? OrderId { get; init; }
}

/// <summary>
/// VietQR account information
/// </summary>
public record VietQRAccountInfo
{
    public string BankBin { get; init; } = string.Empty;
    public string AccountNo { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public string ApiUrl { get; init; } = string.Empty;
}
