using Eventuous;
using NodaTime;

namespace Bookings.Domain.Bookings;

public static class BookingEvents {
    public static class V1 {
        [EventType("V1.RoomBooked")] // https://eventuous.dev/docs/persistence/serialisation/#auto-registration-of-types
        public record RoomBooked(
            string         GuestId,
            string         RoomId,
            LocalDate      CheckInDate,
            LocalDate      CheckOutDate,
            float          BookingPrice,
            float          PrepaidAmount,
            float          OutstandingAmount,
            string         Currency,
            DateTimeOffset BookingDate
        );

        //todo? RoomChanged + StayPeriod changed ? 
        [EventType("V1.BookingChanged")]
        public record BookingChanged(
            string BookingId,            
            string RoomId,
            LocalDate CheckInDate,
            LocalDate CheckOutDate,
            float BookingPrice,
            float PrepaidAmount,
            float OutstandingAmount,
            string Currency, //currency and price are bound, they are "Money"
            DateTimeOffset BookingDate
        );


        [EventType("V1.PaymentRecorded")]
        public record PaymentRecorded(
            float          PaidAmount,
            float          Outstanding,
            string         Currency,
            string         PaymentId,
            string         PaidBy,
            DateTimeOffset PaidAt
        );

        [EventType("V1.FullyPaid")]
        public record BookingFullyPaid(DateTimeOffset FullyPaidAt);

        [EventType("V1.Overpaid")]
        public record BookingOverpaid(DateTimeOffset OverpaidAt);

        [EventType("V1.BookingCancelled")]
        public record BookingCancelled(string CancelledBy, DateTimeOffset CancelledAt);
    }
}