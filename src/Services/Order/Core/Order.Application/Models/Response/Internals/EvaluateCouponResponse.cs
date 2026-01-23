namespace Order.Application.Models.Response.Internals;

public class EvaluateCouponResponse
{

    #region Fields, Properties and Indexers

    public decimal OriginalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string CouponCode { get; set; }

    #endregion
}