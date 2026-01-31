using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Data;
using Payment.Application.Gateways;
using Payment.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Payment.Worker.Jobs;

public class ReconcilePaymentBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReconcilePaymentBackgroundService> _logger;

    public ReconcilePaymentBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ReconcilePaymentBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReconcilePaymentBackgroundService starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReconcileAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Reconcile Job");
            }

            // Run every 5 minutes
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
        
        _logger.LogInformation("ReconcilePaymentBackgroundService stopping...");
    }

    private async Task ReconcileAsync(CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var gatewayFactory = scope.ServiceProvider.GetRequiredService<IPaymentGatewayFactory>();

        // 1. Find payments pending > 15 minutes
        var cutOffTime = DateTimeOffset.UtcNow.AddMinutes(-15);
        var pendingPayments = await dbContext.Payments
            .Where(p => (p.Status == PaymentStatus.Processing || p.Status == PaymentStatus.Pending)
                        && p.CreatedOnUtc < cutOffTime)
            .Take(50) // Batch size
            .ToListAsync(token);

        if (!pendingPayments.Any()) return;

        _logger.LogInformation("Found {Count} pending payments to reconcile", pendingPayments.Count);

        foreach (var payment in pendingPayments)
        {
            try
            {
                // 2. Ask Gateway
                var gateway = gatewayFactory.GetGateway(payment.Method);

                // If payment doesn't have TransactionId yet (error during creation), fallback to Id.
                // Note: Gateway implementation determines how to handle this.
                var transactionIdToVerify = !string.IsNullOrEmpty(payment.TransactionId) 
                    ? payment.TransactionId 
                    : payment.Id.ToString();

                var verifyResult = await gateway.VerifyPaymentAsync(transactionIdToVerify, token);

                if (verifyResult.IsSuccess)
                {
                    // Gateway says Success -> Update Completed
                    _logger.LogInformation("Payment {Id} found Success at Gateway. Updating...", payment.Id);
                    
                    // Assuming 'System' actor for background job
                    payment.Complete(verifyResult.TransactionId!, "Reconciled", "System:Worker");
                }
                else
                {
                    // Gateway says Fail or Not Found -> Mark Failed
                    // BE CAREFUL: "Not Found" might mean it never existed OR it's just not found.
                    // For now, if verify fails, we treat it as failed payment.
                    _logger.LogWarning("Payment {Id} verify failed at Gateway. Message: {Message}. Updating to Failed...", payment.Id, verifyResult.ErrorMessage);
                    
                    payment.MarkAsFailed(
                        "RECONCILE_FAILED", 
                        verifyResult.ErrorMessage ?? "Gateway verification failed", 
                        verifyResult.RawResponse, // If available
                        "System:Worker");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconcile payment {Id}", payment.Id);
            }
        }

        await dbContext.SaveChangesAsync(token);
    }
}
