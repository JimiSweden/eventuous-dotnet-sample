using Bookings.Domain.Bookings;
using Eventuous.Projections.MongoDB;
using MongoDB.Driver;

namespace Bookings.Application.Queries;

/// <summary>
/// (not really needed, just for exploring) <br/>
/// testing projecting to a "fixed name" document for fast read of "something",
/// in this example all guests with bookings (ever created) are listed. 
/// </summary>
public class StatisticsOfBookingsProjection : MongoProjection<StatisticsOfBookingsDocument>
{
    public StatisticsOfBookingsProjection(IMongoDatabase database) : base(database)
    {
        On<BookingEvents.V1.RoomBooked>(builder => builder
            .UpdateOne
            .Id( _ => "statistics") //static Id to always add guests to the same document
            .UpdateFromContext((ctx, update) =>
                //AddToSet will only add unique values, no duplicates.
                update.AddToSet(
                    x => x.Guests, new StatisticsOfBookingsDocument.GuestWithBookings(ctx.Message.GuestId)
                )
            )
        );
        
    }
}