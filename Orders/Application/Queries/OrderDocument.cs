using Eventuous.Projections.MongoDB.Tools;

namespace Orders.Application.Queries;

public record OrderDocument : ProjectedDocument
{
    public OrderDocument(string id) : base(id) { }
    public string CustomerId { get; set; }

    public string Currency { get; init; }
    public float Price { get; init; }
    public float Outstanding { get; init; }
    public float Discount { get; init; }
    public bool Paid { get; init; }
    public float PrepaidAmount { get; set; }


    /// <summary>
    /// when initial order was created
    /// </summary>
    public DateTimeOffset OrderCreatedDate { get; set; }

    /// <summary>
    /// When order was submitted (as finished/book) from customer
    /// </summary>
    public DateTimeOffset OrderBookedDate { get; set; }
    /// <summary>
    /// if not booked, it is still open for changes (like a in a customer shopping-cart)
    /// </summary>
    public bool Booked { get; init; }

    public bool Cancelled { get; init; }
    public DateTimeOffset OrderCancelledDate { get; set; }
    public string CancelledBy { get; set; }
    public string CancelledReason { get; set; }

    public List<OrderRow> OrderRows { get; set; } = new();

    public record OrderRow(
        string OrderRowId, 
        string ProductId,
        string ProductName,
        string ProductType,
        int ProductAmount);
}