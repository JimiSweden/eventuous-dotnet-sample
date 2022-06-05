using System.Collections.Immutable;
using Eventuous;
using NodaTime;

namespace Orders.Domain.Orders;

public record PaymentRecord(string PaymentId, Money PaidAmount);
public record DiscountRecord(Money Discount, string Reason);

/// <summary>
/// OrderState is used to build the aggregate state from events <br/>
/// record, instead of class, ensures Imutability. it is always 'newed'. <br/>
/// Important note:
/// No business-logic-validations or altering of a state is "possible".<br/>
/// The current state is the truth from the events in history.<br/>
/// Business validations are made in the Aggregate (<see cref="Order"/>.cs)<br/>
/// Before Applying the event (creating/adding the changes of this states history)
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

    public Money Price { get; init; }
    public Money Outstanding { get; init; }
    public Money Discount { get; init; }
    public bool Paid { get; init; }


    /* a note on DateTimeOffset vs DateTime
     * The DateTimeOffset structure represents a date and time value,
     * together with an offset that indicates how much that value differs from UTC.
     * Thus, the value always unambiguously identifies a single point in time.
     * The DateTimeOffset type includes all of the functionality of the DateTime type
     * along with time zone awareness. 
     */

    /// <summary>
    /// when initial order was created
    /// perhaps not needed? if using date from EventStore TimeStamp
    /// </summary>
    public DateTimeOffset OrderCreatedDate { get; set; }

    /* TODO ? perhaps refactor to a "OrderBookingState" containing
     * Booked, Cancelled (and related info), ReOpened ?
     *
     */

    /// <summary>
    /// When order was submitted (as finished/book) from customer
    /// perhaps not needed? if using date from EventStore TimeStamp
    /// </summary>
    public DateTimeOffset OrderBookedDate { get; set; }
    /// <summary>
    /// if not booked, it is still open for changes (like a in a customer shopping-cart)
    /// </summary>
    public bool Booked { get; init; }
    
    public DateTimeOffset OrderCancelledDate { get; set; }
    public bool Cancelled { get; init; }
    public string  CancelledBy { get; set; }
    public string  CancelledReason { get; set; }

    //todo: OrderFullFilled (completed, end-state), OrderDelivered, OrderShipped

    public ImmutableList<OrderRow> OrderRows { get; set; } = ImmutableList<OrderRow>.Empty;
    public ImmutableList<PaymentRecord> PaymentRecords { get; init; } = ImmutableList<PaymentRecord>.Empty;

    /// <summary>
    /// ensure only unique payments.
    /// </summary>
    /// <param name="paymentId"></param>
    /// <returns></returns>
    internal bool HasPaymentBeenRecorded(string paymentId) => PaymentRecords.Any(x => x.PaymentId == paymentId);

    //TODO: Invoice- , Shipping Address and Delivery options
    // bool ShipToCustomerAddress, || ShippingAddress
    public ShippingAddress ShippingAddress{ get; set; }
    public InvoiceAddress InvoiceAddress{ get; set; }


    /// <summary>
    /// note: the state will run through all events
    /// when fetching the order from EventStoreDB,
    /// The events are stored by the aggregate Order, in the Apply methods
    /// </summary>
    public OrderState()
    {
        //from order.AddOrder
        On<OrderEvents.V1.OrderAdded>(HandleAdded);

        //from order.BookOrder
        On<OrderEvents.V1.OrderBooked>(HandleBooked);

        //from order.UnBookOrder
        On<OrderEvents.V1.OrderUnBooked>(HandleUnBooked);

        //from order.CancelOrder
        On<OrderEvents.V1.OrderCancelled>(HandleCancelled);

        //from order.RecordPayment
        On<OrderEvents.V1.PaymentRecorded>(HandlePayment);

        //from order.RecordPayment / MarkFullyPaidIfNecessary
        On<OrderEvents.V1.OrderFullyPaid>((state, paid) => 
            state with { Paid = true });
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
            OrderCreatedDate = added.OrderCreatedDate
        };
    }

    private OrderState HandleBooked(OrderState state, OrderEvents.V1.OrderBooked booked)
    {
        return state with
        {
            Booked = true, //since this a Handler reading Events we know it is booked.
            Price = new Money(booked.OrderPrice, booked.Currency),
            Outstanding = new Money(booked.OutstandingAmount, booked.Currency),
            Discount = new Money(booked.DiscountAmount, booked.Currency),
            OrderBookedDate = booked.OrderBookedDate,
            
            // Note: payment if direct should be set before Booking allowed, thus no need to 
            //Paid = Outstanding.Amount == 0, 
            //todo: shipping address
            //todo: invoice address ? perhaps.. probably 
        };
    }

    private OrderState HandleCancelled(OrderState state, OrderEvents.V1.OrderCancelled cancelled)
    {
        return state with
        {
            Cancelled = true,
            CancelledBy = cancelled.CancelledBy,
            CancelledReason = cancelled.Reason,
            OrderCancelledDate = cancelled.OrderCancelledAt
        };
    }

    private OrderState HandleUnBooked(OrderState state, OrderEvents.V1.OrderUnBooked unBooked)
    {
        return state with
        {
            Booked = false,
            OrderBookedDate = default,
        };
    }

    //todo: HandleUnbooked, set Booked = false, .


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

}