using System.Collections.Immutable;

namespace Orders.Domain.Orders;

public record OrderRow
{
    public OrderRowId Id { get; set; }
    public string OrderId { get; set; }
        
    /// <summary>
    /// order row items are Products av some Type (Service or Goods) 
    /// </summary>
    public ImmutableList<Product> OrderRowItems { get; set; }
}