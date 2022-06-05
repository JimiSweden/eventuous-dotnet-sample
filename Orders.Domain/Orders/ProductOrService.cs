namespace Orders.Domain.Orders;

public record ProductOrService
{
    public  ProductOrServiceId Id { get; set; }

    //todo: Type of prod/service with UnitType (time, weight, fluids etc..)
    // colors and sizes etc would (?) be handled with different productIds.
    public string Type { get; set; } 
    public string Name { get; set; }

    /// <summary>
    /// TODO: perhaps not have a description here? perhaps too much?
    /// but.. short descriptions can 
    /// </summary>
    public string Description { get; set; }
    public Money Price { get; init; }

    //todo? add discontinued etc ?
    // if product cannot be ordered anymore... 
    // if available in stock etc would be handled by an inventory service
    // inventory service would also "own" the products/services.
    // Order service should no know about such details.

    public ProductOrService(ProductOrServiceId id, string type, string name, string description, Money price)
    {
        Id = id;
        Type = type;
        Name = name;
        Description = description;
        Price = price;
    }
}