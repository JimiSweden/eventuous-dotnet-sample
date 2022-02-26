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
        On<V1.RoomBooked>(
            evt => evt.GuestId,
            
            (evt, update) => update.AddToSet(
                x => x.Bookings,
                new MyBookings.Booking(evt.BookingId, evt.CheckInDate, evt.CheckOutDate, evt.BookingPrice)
            )
        );
    }
}