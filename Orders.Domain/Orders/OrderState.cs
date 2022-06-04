using System.Collections.Immutable;
using Eventuous;
using NodaTime;

namespace Orders.Domain.Orders;

public record PaymentRecord(string PaymentId, Money PaidAmount);


/// <summary>
/// OrderState is used to build the aggregate state from events <br/>
/// record, instead of class, ensures Imutability. it is always 'newed'
/// </summary>
public record OrderState : AggregateState<OrderState, OrderId>
{

    /*
     * NOTE: setting an Id here will override the inherited AggregateState Id
     * and the following error will be thrown during Apply(...). in the Order class in this case. (inheriting Aggregate)
     * "errorMessage": "Aggregate id AggregateId cannot have an empty value",
     *
     * It is hard to find /catch the error (because it runs on another thread?)
     * I could not catch it with a try-catch block around 
     *
     */
    //public OrderId Id { get; set; }
    public string CustomerId { get; set; }

    /// <summary>
    /// When order was submitted from customer
    /// perhaps not needed? if using date from EventStore TimeStamp
    /// </summary>
    public LocalDate OrderBookedDate { get; set; }
        
    /// <summary>
    /// when initial order was created
    /// </summary>
    public DateTime OrderCreatedDate { get; set; }
    public Money Price { get; init; }
    public Money Outstanding { get; init; }
    public Money Discount { get; init; }
    public bool Paid { get; init; }
    /// <summary>
    /// if not booked, it is still open (like a in a customer cart)
    /// </summary>
    public bool Booked { get; init; }

    public ImmutableList<OrderRow> OrderRows { get; set; } = ImmutableList<OrderRow>.Empty;
    public ImmutableList<PaymentRecord> PaymentRecords { get; init; } = ImmutableList<PaymentRecord>.Empty;
    internal bool HasPaymentBeenRecorded(string paymentId) => PaymentRecords.Any(x => x.PaymentId == paymentId);

    //TODO: Invoice- , Shipping Address and Delivery options
    // bool ShipToCustomerAddress, || ShippingAddress
    public ShippingAddress ShippingAddress{ get; set; }
    public InvoiceAddress InvoiceAddress{ get; set; }


    public OrderState()
    {
        On<OrderEvents.V1.OrderAdded>(HandleAdded);
        On<OrderEvents.V1.OrderBooked>(HandleBooked);

        On<OrderEvents.V1.PaymentRecorded>(HandlePayment);
        On<OrderEvents.V1.OrderFullyPaid>((state, paid) => state with { Paid = true });
    }


    /* a note on records "with"
     * 'record with' will "modify" the props set here,
     *  and use the current props as is from current values
     * , in our case, our OrderState 'state'
     *  and return a new record. (records are immutable)
     */

    private OrderState HandleAdded(OrderState state, OrderEvents.V1.OrderAdded added)
    {
        return state with
        {
            Id = new OrderId(added.OrderId),
            CustomerId = added.CustomerId,
            OrderCreatedDate = added.OrderCreatedDate.LocalDateTime
        };
    }

    private OrderState HandleBooked(OrderState state, OrderEvents.V1.OrderBooked booked)
    {
        return state with
        {
            Price = new Money(booked.OrderPrice, booked.Currency),
            Outstanding = new Money(booked.OutstandingAmount, booked.Currency),
            Discount = new Money(booked.DiscountAmount, booked.Currency),
            OrderBookedDate = booked.OrderBookedDate
            //todo: shipping address
            //todo: invoice address ? perhaps.. probably 
        };
    }
    private OrderState HandlePayment(OrderState state, OrderEvents.V1.PaymentRecorded payment)
    {
        return state with
        {
            Outstanding = new Money(payment.Outstanding, payment.Currency),
            PaymentRecords = state.PaymentRecords.Add(
                new PaymentRecord(payment.PaymentId, new Money(payment.PaidAmount, payment.Currency))
                )
        };
    }
    //static BookingState HandlePayment(BookingState state, V1.PaymentRecorded e)
    //    
    //    => state with
    //    {
    //        Outstanding = new Money { Amount = e.Outstanding, Currency = e.Currency },
    //        PaymentRecords = state.PaymentRecords.Add(
    //            new PaymentRecord(e.PaymentId, new Money { Amount = e.PaidAmount, Currency = e.Currency })
    //        )
    //    };
}