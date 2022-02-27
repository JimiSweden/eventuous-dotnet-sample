using Bookings.Domain;
using Bookings.Domain.Bookings;
using Eventuous;
using NodaTime;
using static Bookings.Application.BookingCommands;

namespace Bookings.Application;

public class BookingsCommandService : ApplicationService<Booking, BookingState, BookingId> {
    
    /// <summary>
    /// TODO: Booking Id should be generated, UUID?, and prefixed with current date.
    /// </summary>
    /// <param name="store"></param>
    /// <param name="isRoomAvailable"></param>
    public BookingsCommandService(IAggregateStore store, Services.IsRoomAvailable isRoomAvailable) : base(store) {
        OnNewAsync<BookRoom>(
            (booking, cmd, _) => booking.BookRoom(
                new BookingId(cmd.BookingId),
                cmd.GuestId,
                new RoomId(cmd.RoomId),
                new StayPeriod(LocalDate.FromDateTime(cmd.CheckInDate), LocalDate.FromDateTime(cmd.CheckOutDate)),
                new Money(cmd.BookingPrice, cmd.Currency),
                new Money(cmd.PrepaidAmount, cmd.Currency),
                DateTimeOffset.Now,
                isRoomAvailable //NOTE: isRoomAvalable is the service delegate method, with implementation registered in Registrations
            )
        );

        /* OnExisting operates on the Id to get the existing booking, 
         * then the booking.Change uses the State.Id internally
         */
        OnExistingAsync<ChangeBooking>(
            cmd => new BookingId(cmd.BookingId),
            (booking, cmd, _) => booking.Change(
                new RoomId(cmd.RoomId),
                new StayPeriod(LocalDate.FromDateTime(cmd.CheckInDate), LocalDate.FromDateTime(cmd.CheckOutDate)),
                new Money(cmd.BookingPrice, cmd.Currency),
                new Money(cmd.PrepaidAmount, cmd.Currency),
                DateTimeOffset.Now,
                isRoomAvailable //NOTE: isRoomAvalable is the service delegate method, with implementation registered in Registrations
            )
        );

        OnExisting<RecordPayment>(
            cmd => new BookingId(cmd.BookingId),
            (booking, cmd) => booking.RecordPayment(
                new Money(cmd.PaidAmount, cmd.Currency),
                cmd.PaymentId,
                cmd.PaidBy,
                DateTimeOffset.Now
            )
        );

        
    }
}