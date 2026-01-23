using Discount.Grpc;
using Microsoft.Extensions.Logging;
using Order.Application.Services;

namespace Order.Infrastructure.Services;

public sealed class DiscountGrpcService(
    DiscountGrpc.DiscountGrpcClient grpcClient,
    ILogger<DiscountGrpcService> logger) : IDiscountGrpcService
{

    public async Task<Order.Application.Models.Response.Internals.ApplyCouponResponse?> ApplyCouponAsync(string couponCode, decimal amount, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ApplyCouponRequest { Amount = (double)amount, Code = couponCode };

            var result = await grpcClient.ApplyCouponAsync(
                request,
                cancellationToken: cancellationToken);

            return new Order.Application.Models.Response.Internals.ApplyCouponResponse()
            {
                CouponCode = result.CouponCode
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to apply coupon {CouponCode} from Discount Grpc service", couponCode);
            return null;
        }
    }

    public async Task<Order.Application.Models.Response.Internals.EvaluateCouponResponse?> EvaluateCouponAsync(string couponCode, decimal amount, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new EvaluateCouponRequest { Amount = (double)amount, Code = couponCode };

            var result = await grpcClient.EvaluateCouponAsync(
                request,
                cancellationToken: cancellationToken);

            return new Order.Application.Models.Response.Internals.EvaluateCouponResponse()
            {
                CouponCode = result.CouponCode,
                DiscountAmount = (decimal)result.DiscountAmount,
                FinalAmount = (decimal)result.FinalAmount,
                OriginalAmount = (decimal)result.OriginalAmount,
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to evaluate coupon {CouponCode} from Discount Grpc service", couponCode);
            return null;
        }
    }

}