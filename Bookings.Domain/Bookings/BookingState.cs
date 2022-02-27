using System.Collections.Immutable;
using Eventuous;
using static Bookings.Domain.Bookings.BookingEvents;

namespace Bookings.Domain.Bookings;

/// <summary>
/// BookingState is used to build the aggregate state from events <br/>
/// record, instead of class, ensures Imutability. it is always 'newed'
/// </summary>
public record BookingState : AggregateState<BookingState, BookingId>
{
    public string GuestId { get; init; }
    public RoomId RoomId { get; init; }
    public StayPeriod Period { get; init; }
    public Money Price { get; init; }
    public Money Outstanding { get; init; }
    public bool Paid { get; init; }

    public ImmutableList<PaymentRecord> PaymentRecords { get; init; } = ImmutableList<PaymentRecord>.Empty;

    internal bool HasPaymentBeenRecorded(string paymentId) => PaymentRecords.Any(x => x.PaymentId == paymentId);

    public BookingState() {
        On<V1.RoomBooked>(HandleBooked);
        On<V1.BookingChanged>(HandleBookingChanged);
        On<V1.PaymentRecorded>(HandlePayment);
        On<V1.BookingFullyPaid>((state, paid) => state with { Paid = true });
    }

    static BookingState HandlePayment(BookingState state, V1.PaymentRecorded e)
        // 'record with' will "modify" the props set here, and use the current props as is from, in our case, our BookingState 'state' 
        => state with {
            Outstanding = new Money { Amount = e.Outstanding, Currency = e.Currency },
            PaymentRecords = state.PaymentRecords.Add(
                new PaymentRecord(e.PaymentId, new Money { Amount = e.PaidAmount, Currency = e.Currency })
            )
        };

    static BookingState HandleBooked(BookingState state, V1.RoomBooked booked)
        => state with {
            Id = new BookingId(booked.BookingId),
            RoomId = new RoomId(booked.RoomId),
            Period = new StayPeriod(booked.CheckInDate, booked.CheckOutDate),
            GuestId = booked.GuestId,
            Price = new Money { Amount = booked.BookingPrice, Currency = booked.Currency },
            Outstanding = new Money { Amount = booked.OutstandingAmount, Currency = booked.Currency }
        };

    static BookingState HandleBookingChanged(BookingState state, V1.BookingChanged changed)
        => state with
        {
            RoomId = new RoomId(changed.RoomId),
            Period = new StayPeriod(changed.CheckInDate, changed.CheckOutDate),
            Price = new Money { Amount = changed.BookingPrice, Currency = changed.Currency },
            Outstanding = new Money { Amount = changed.OutstandingAmount, Currency = changed.Currency }
        };
}

public record PaymentRecord(string PaymentId, Money PaidAmount);

public record DiscountRecord(Money Discount, string Reason);