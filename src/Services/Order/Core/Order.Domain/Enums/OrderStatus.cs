using System.ComponentModel;

namespace Order.Domain.Enums;

public enum OrderStatus
{

    #region Fields, Properties and Indexers
    [Description("Pending")] Pending = 1,
    [Description("Confirmed")] Confirmed = 2,
    [Description("Processing")] Processing = 3,
    [Description("Shipped")] Shipped = 4,
    [Description("Delivered")] Delivered = 5,
    [Description("Cancelled")] Cancelled = 6,
    [Description("Returned")] Returned = 7
    #endregion
}