using Eventuous;
using static Bookings.Domain.Bookings.BookingEvents;
using static Bookings.Domain.Services;

namespace Bookings.Domain.Bookings;

/// <summary>
/// note that the actions/methods here needs to be handled in <see cref="Bookings.Application.BookingsCommandService"/>
/// </summary>
public class Booking : Aggregate<BookingState> {
    public async Task BookRoom(
        string guestId,
        RoomId roomId,
        StayPeriod period,
        Money price,
        Money prepaid,
        DateTimeOffset bookedAt,
        IsRoomAvailable isRoomAvailable
    )
    {
        //from the Eventuous base class 'Aggregate' and checks that the CurrentVersion is not set, will throw a 'DomainException' if it does
        EnsureDoesntExist();

        //throws DomainException if room is not available
        await EnsureRoomAvailable(roomId, period, isRoomAvailable);

        //Note: since price ans prepaid are records, the '-' operator is a method in Money,
        // containing business logic, i.e. validaion: (you can't subtract on different currencies)
        var outstanding = price - prepaid;

        /* Apply comes from the Eventuous abstract class Aggregate<T> : Aggregate where T : AggregateState<T>
         *  meaning it has a AggregateState, as the signature for our BookingState
         *  public record BookingState : AggregateState<BookingState, BookingId>
         *  
         *  Apply will add the event,V1.RoomBooked, to the internal State._changes
         *  
         *  it returns versions of the state before and after event is applied (but you probably don't need them)
         *  
        */
        
        Apply(
            new V1.RoomBooked(
                guestId,
                roomId,
                period.CheckIn,
                period.CheckOut,
                price.Amount,
                prepaid.Amount,
                outstanding.Amount,
                price.Currency,
                bookedAt
            )
        );


        MarkFullyPaidIfNecessary(bookedAt);
    }


    public async Task Change(
        RoomId roomId,
        StayPeriod period,
        Money price,
        Money prepaid,//needed for price change and calculating outstanding,
                      // could be calculated from State.outstanding and State.price before (or adding a prepaidAmount prop)
        DateTimeOffset bookedAt,
        IsRoomAvailable isRoomAvailable
    )
    {
        EnsureExists();

        //throws DomainException if room is not available
        await EnsureRoomAvailable(roomId, period, isRoomAvailable);

        //Note: since price and prepaid are records, the '-' operator is a method in Money,
        // containing business logic, i.e. validaion: (you can't subtract on different currencies)
        var outstanding = price - prepaid;

        Apply(new V1.BookingChanged(
              // State.Id, //bookingId,
               roomId,
               period.CheckIn,
               period.CheckOut,
               price.Amount,
               prepaid.Amount,
               outstanding.Amount,
               price.Currency, //currency and price are bound, they are "Money"
               bookedAt
               )
           );
    }

    public void RecordPayment(
        Money paid,
        string paymentId,
        string paidBy,
        DateTimeOffset paidAt
    ) {
        EnsureExists();

        if (State.HasPaymentBeenRegistered(paymentId)) return;

        var outstanding = State.Outstanding - paid;

        Apply(
            new V1.PaymentRecorded(
                paid.Amount,
                outstanding.Amount,
                paid.Currency,
                paymentId,
                paidBy,
                paidAt
            )
        );

        MarkFullyPaidIfNecessary(paidAt);
        MarkOverpaid(paidAt);
    }

    void MarkFullyPaidIfNecessary(DateTimeOffset when)
    {
        if (State.Outstanding.Amount > 0) return;

        Apply(new V1.BookingFullyPaid(when));
    }

    void MarkOverpaid(DateTimeOffset when) {
        if (State.Outstanding.Amount < 0) return;

        Apply(new V1.BookingOverpaid(when));
    }

    static async Task EnsureRoomAvailable(RoomId roomId, StayPeriod period, IsRoomAvailable isRoomAvailable)
    {
        var roomAvailable = await isRoomAvailable(roomId, period);
        if (!roomAvailable) throw new DomainException("Room not available");
    }
}