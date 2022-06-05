using Eventuous.Projections.MongoDB;
using MongoDB.Driver;
using static Bookings.Domain.Bookings.BookingEvents;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Bookings.Application.Queries;

/// <summary>
/// This is used to update the pojection BookingDocument, in MongoDb, 
/// it gets the data from the event-store
/// README : https://github.com/Eventuous/eventuous/tree/ce728bb13fa11ac220b4dfe7d8dc7d8e4b9abf3f/src/Mongo/src/Eventuous.Projections.MongoDB
/// </summary>
public class BookingStateProjection : MongoProjection<BookingDocument> {
    public BookingStateProjection(IMongoDatabase database) : base(database) {
        On<V1.RoomBooked>(evt => evt.BookingId, HandleRoomBooked);

        On<V1.BookingChanged>(evt => evt.BookingId, HandleBookingChanged);

        On<V1.PaymentRecorded>(
            evt => evt.BookingId,
            (evt, update) => update.Set(x => x.Outstanding, evt.Outstanding)
        );

        On<V1.BookingFullyPaid>(
            evt => evt.BookingId,
            (_, update) => update.Set(x => x.Paid, true)
        );
    }

    static UpdateDefinition<BookingDocument> HandleRoomBooked(
        V1.RoomBooked evt, UpdateDefinitionBuilder<BookingDocument> update
    )
        => update.SetOnInsert(x => x.Id, evt.BookingId)
            .Set(x => x.GuestId, evt.GuestId)
            .Set(x => x.RoomId, evt.RoomId)
            .Set(x => x.CheckInDate, evt.CheckInDate)
            .Set(x => x.CheckOutDate, evt.CheckOutDate)
            .Set(x => x.BookingPrice, evt.BookingPrice)
            .Set(x => x.Outstanding, evt.OutstandingAmount);

    static UpdateDefinition<BookingDocument> HandleBookingChanged(
        V1.BookingChanged evt, UpdateDefinitionBuilder<BookingDocument> update
    )
        => update.Set(x => x.RoomId, evt.RoomId)
            .Set(x => x.CheckInDate, evt.CheckInDate)
            .Set(x => x.CheckOutDate, evt.CheckOutDate)
            .Set(x => x.BookingPrice, evt.BookingPrice)
            .Set(x => x.Outstanding, evt.OutstandingAmount);
    

}