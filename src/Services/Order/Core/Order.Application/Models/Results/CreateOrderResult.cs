namespace Order.Application.Models.Results;

public class CreateOrderResult
{

    #region Fields, Properties and Indexers
    public Guid OrderId { get; set; }
    public string OrderNo { get; set; } = default!;
    #endregion

}