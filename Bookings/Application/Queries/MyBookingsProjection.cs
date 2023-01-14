using Eventuous.Projections.MongoDB;
using MongoDB.Driver;
using static Bookings.Domain.Bookings.BookingEvents;

namespace Bookings.Application.Queries;

/// <summary>
/// This updates the projection holding "MyBookings" in MongoDb <br/>
/// - adds the new booking from the RoomBoked event to the current Guests list of Bookings <br/>
/// - note that the data in <see cref="MyBookings.Booking"/> only has some of the data available in <see cref="V1.RoomBooked"/>
/// </summary>
public class MyBookingsProjection : MongoProjection<MyBookings> {
    public MyBookingsProjection(IMongoDatabase database) : base(database) {
        On<V1.RoomBooked>(b => b
            .UpdateOne
            .Id(ctx => ctx.Message.GuestId)
            .UpdateFromContext((ctx, update) =>
                update.AddToSet(
                    x => x.Bookings,
                    new MyBookings.Booking(ctx.Stream.GetId(),
                        ctx.Message.CheckInDate,
                        ctx.Message.CheckOutDate,
                        ctx.Message.BookingPrice
                    )
                )
            )
        );

        On<V1.BookingCancelled>(
            b => b.UpdateOne
                .Filter((ctx, doc) =>
                    doc.Bookings.Select(booking => booking.BookingId).Contains(ctx.Stream.GetId())
                )
                .UpdateFromContext((ctx, update) =>
                    update.PullFilter(
                        x => x.Bookings,
                        x => x.BookingId == ctx.Stream.GetId()
                    )
                )
        );
    }

    //TODO: update MyBookings.Booking when a booking has changed. V1.BookingChanged
}
