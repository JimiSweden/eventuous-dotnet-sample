namespace Bookings.Application;

public static class BookingCommands {
    public record BookRoom(
        string         BookingId,
        string         GuestId,
        string         RoomId,
        DateTime       CheckInDate,
        DateTime       CheckOutDate,
        float          BookingPrice,
        float          PrepaidAmount,
        string         Currency,
        DateTimeOffset BookingDate
    );

    public record ChangeBooking(
        string BookingId,
        string RoomId,
        DateTime CheckInDate,
        DateTime CheckOutDate,
        float BookingPrice,
        float PrepaidAmount, //todo: remove? 
        string Currency, //currency and price are bound, they are "Money"
        DateTimeOffset BookingDate
    );

    public record RecordPayment(string BookingId, float PaidAmount, string Currency, string PaymentId, string PaidBy);
}