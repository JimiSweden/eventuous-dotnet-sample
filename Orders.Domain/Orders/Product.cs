namespace Orders.Domain.Orders;

public record Product
{
    public  ProductId Id { get; set; }
    public string ProductType { get; set; } //todo: Typed?
    public string Name { get; set; }
    public string Description { get; set; }
    public Money Price { get; init; }

}