using Eventuous.Projections.MongoDB;
using Eventuous.Subscriptions.Context;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using static Bookings.Domain.Bookings.BookingEvents;

namespace Bookings.Application.Queries;

/// <summary>
/// This updates the projection holding "MyBookings" in MongoDb <br/>
/// - adds the new booking from the RoomBoked event to the current Guests list of Bookings <br/>
/// - note that the data in <see cref="MyBookings.Booking"/> only has some of the data available in <see cref="V1.RoomBooked"/>
/// </summary>
public class MyBookingsProjection : MongoProjection<MyBookings>
{
    public MyBookingsProjection(IMongoDatabase database) : base(database)
    {
        //Adds a Booking to the array of Bookings
        On<V1.RoomBooked>(b => b
            .UpdateOne
            .Id(ctx => ctx.Message.GuestId)
            .UpdateFromContext((ctx, update) =>
                update.AddToSet(
                    x => x.Bookings,
                    new MyBookings.Booking(ctx.Stream.GetId(),
                        ctx.Message.CheckInDate,
                        ctx.Message.CheckOutDate,
                        ctx.Message.BookingPrice)
                    )
                )
        );

        On<V1.BookingChanged>(MatchBookingInMyBookingsBookings, HandleBookingChanged);

        //when a booking is cancelled we choose to remove it from "my bookings",
        //if we would like to still have it in the list for history we might just update a property "IsCancelled"
        //, or set a CancelledDate,
        //and perhaps move the booking to a new array like MyBookings.BookingsCancelled, or MyBookings.HistoricalBookings
        // or make a separate document like MyBookingsWithHistory. It all depends on our need for the views :)
        On<V1.BookingCancelled>(
            b => b.UpdateOne
                .Filter((ctx, doc) =>
                    doc.Bookings.Select(booking => booking.BookingId).Contains(ctx.Stream.GetId())
                )
                .UpdateFromContext((ctx, update) =>
                    update.PullFilter(x => x.Bookings, x => x.BookingId == ctx.Stream.GetId()
                    )
                )
        );
    }

    /// <summary>
    /// Updates the document, in this case used with the filter <see cref="MatchBookingInMyBookingsBookings"/>
    /// <br/>
    /// Using "pull filter" - to remove element from array inside a document
    /// the array positional parameter '-1' (minus one) might look wrong or strange, but it is translated to '$' used for positioning in MongoDB <br/>
    /// (note: For more info on filtering Arrays see "README resources tips n tricks.md" in root)
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="update"></param>
    /// <returns></returns>
    private UpdateDefinition<MyBookings> HandleBookingChanged(IMessageConsumeContext<V1.BookingChanged> ctx,
        UpdateDefinitionBuilder<MyBookings> update)
    {
        return update
            .Set(x => x.Bookings[-1].CheckInDate, ctx.Message.CheckInDate)
            .Set(x => x.Bookings[-1].CheckOutDate, ctx.Message.CheckOutDate)
            .Set(x => x.Bookings[-1].Price, ctx.Message.BookingPrice);
    }


    /// <summary>
    /// a filter to find (include) what is to be updated
    /// Note that in this case we do not need the Id of the document MyBookings holding the nested Booking we are updating.
    /// The filter will go through all MyBooking documents
    /// and filter out all documents containing a Booking with the Id we get from the stream (BookingState/stream)
    /// Since the Booking can only be found in one "MyBookings" it's enough. <br/>
    /// Performance wise it is perhaps a good idea to also match on the document,
    /// since this will be a faster search; not walking through the content of all MyBookings documents.
    /// Also, if you are updating "rows" or nested Items that could be the same in multiple documents
    /// where you should only update a specific document (f ex for a specific user) you have to make sure to include the document Id in the command.
    /// f ex in this example we could include the "guestId" in "BookingChanged" command to ensure we only get one document in our filter.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    private FilterDefinition<MyBookings> MatchBookingInMyBookingsBookings(IMessageConsumeContext<V1.BookingChanged> ctx, FilterDefinitionBuilder<MyBookings> filter)
    {
        //get the document holding the nested array of "Bookings" : Not needed in this case, but kept for reference.
        //var documentFilter = new ExpressionFilterDefinition<MyBookings>(x => x.Id == "MyBookingsDocumentId"); 

        //match array element from Bookings on bookingId :
        //this is equvalent to LINQ expression like
        //MyBookings.Where(my => my.Bookings.Any(x.BookingId == bookingId));
        var bookingFilterDefinition = new FilterDefinitionBuilder<MyBookings.Booking>().Eq(x => x.BookingId, ctx.Stream.GetId());

        //match field Bookings, with the Equality filter for matching on Id
        //(matches a single row/element/field in a single document in this case
        //but could in theory be more than one "element" in a document (or multiple documents)
        //if they have a Booking element with the same id)
        var documentBookingsFilter = new FilterDefinitionBuilder<MyBookings>().ElemMatch(x => x.Bookings, bookingFilterDefinition);

        // an AND filter is needed to first match the main document(MyBookings) and then the array element type "Booking"
        //however, in this example I chose to not filter on the document (see summary above)
        return filter.And(new List<FilterDefinition<MyBookings>>
        {
            //documentFilter, //not needed if since the bookingId for the array element will be "good enough" for finding the document
            documentBookingsFilter
        });

    }
    
}
