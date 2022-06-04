namespace Orders.Domain;

public record ShippingAddress
{
    //Notes, i.e. mottagare, portkod etc. 

    public string Name { get; set; }
    public string Address { get; set; }
    public string PostCode { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string EmailOfReceiver { get; set; }
    public string PhoneOfReceiver { get; set; }


    //todo: validations..
}

/// <summary>
/// should be connected to customer details
/// </summary>
public record InvoiceAddress
{
    //Notes, i.e. mottagare, portkod etc. 

    public string Name { get; set; }
    public string Address { get; set; }
    public string PostCode { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }

    //todo: validations..
}