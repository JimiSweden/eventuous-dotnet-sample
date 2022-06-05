using System.Collections.Immutable;

namespace Orders.Domain.Orders;

public record OrderRow
{
    public OrderRowId Id { get; set; }
    //public string OrderId { get; set; } //not needed since row lives in Order
        
    /// <summary>
    /// order row items are Products av some Type (Service or Goods) 
    /// </summary>
    public ProductOrService ProductOrService { get; set; }

    /// <summary>
    /// number of items
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// All items packed or reserved (at warehouse or similar)
    /// </summary>
    public bool FulFilled { get; set; }
    public DateTimeOffset FulFilledDate { get; set; }

    /// <summary>
    /// if not fulfilled
    /// </summary>
    public int OutstandingAmount { get; set; }

    public OrderRow(OrderRowId id, ProductOrService productOrService, int amount)
    {
        Id = id;
        ProductOrService = productOrService;
        Amount = amount;
    }
}