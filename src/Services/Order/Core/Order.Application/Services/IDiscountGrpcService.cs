using Order.Application.Models.Response.Internals;

namespace Order.Application.Services;

public interface IDiscountGrpcService
{

    #region Methods

    Task<ApplyCouponResponse?> ApplyCouponAsync(string couponCode, decimal amount, CancellationToken cancellationToken = default);
    Task<EvaluateCouponResponse?> EvaluateCouponAsync(string couponCode, decimal amount, CancellationToken cancellationToken = default);

    #endregion
}