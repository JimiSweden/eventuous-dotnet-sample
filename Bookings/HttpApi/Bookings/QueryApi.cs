
using System.Diagnostics;
using Bookings.Application.Queries;
using Bookings.Domain.Bookings;
using Eventuous;
using Eventuous.Projections.MongoDB.Tools;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Bookings.HttpApi.Bookings;

[Route("/bookings")]
public class QueryApi : ControllerBase {
    readonly IAggregateStore _store;
    readonly IMongoDatabase _mongoDb;
    

    public QueryApi(IAggregateStore store, IMongoDatabase mongoDb)
    {
        _store = store;
        _mongoDb = mongoDb;
        /*
         * Read this for mongo db operations from dotnet
         * https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-6.0&tabs=visual-studio#add-a-crud-operations-service
         */
    }

    /// <summary>
    /// Get a booking from Eventstore
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{id}")]
    public async Task<BookingState> GetBooking(string id, CancellationToken cancellationToken) {
        //This reads from the AggregateStore, that is the EventStoreDB in our case (default)
        var booking = await _store.Load<Booking>(StreamName.For<Booking>(id), cancellationToken);
        return booking.State;
    }

    /// <summary>
    /// all Bookings
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    //[Route("all")]

    public async Task<IEnumerable<BookingDocument>> GetAllBookings(CancellationToken cancellationToken)
    {
        //using mongo db
        var collection = _mongoDb.GetCollection<BookingDocument>("Booking");
        if (collection == null)
        {
            Debugger.Break();
        }
        var gustBookings = await collection.Find(_ => true).ToListAsync(cancellationToken);

        return gustBookings;

    }

    /// <summary>
    /// Get MyBookings-projection from MongoDb for a single guest/user.
    /// </summary>
    /// <param name="guestId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("guest/{guestId}")]
    public async Task<MyBookings> GetMyBookings(string guestId, CancellationToken cancellationToken)
    {
        //using mongo db
        var collection = _mongoDb.GetCollection<MyBookings>("MyBookings");
        if (collection == null)
        {
            Debugger.Break();
        }
        var gustBookings = await collection.Find(x => x.Id == guestId).FirstOrDefaultAsync(cancellationToken);

        //using extension in Eventuous.Projections.MongoDB.Tools
        var myBookingsForGuest = await _mongoDb.LoadDocument<MyBookings>(guestId, cancellationToken);
        return myBookingsForGuest;
    }

    /// <summary>
    /// Get all guests MyBookings-projection from collection in mongoDb
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("GroupedByGuest")]

    public async Task<IEnumerable<MyBookings>> GetAllBookingsGroupedByGuest(CancellationToken cancellationToken)
    {
        //using mongo db
        var collection = _mongoDb.GetCollection<MyBookings>("MyBookings");
        if (collection == null)
        {
            Debugger.Break();
        }
        var gustBookings = await collection.Find(_ => true).ToListAsync(cancellationToken);

        //NOTE: if using mongoDb without the extensino below, we don't need guestIds.
        // this is a playground, comparing alternatives, it would be better to just return gustBookings here.

        var guestIds = await GetGuestIdsForThoseWhoHasEverBooked(cancellationToken);
        //using extension in Eventuous.Projections.MongoDB.Tools
        var allGuestsMyBookings = await _mongoDb.LoadDocuments<MyBookings>(guestIds,cancellationToken);
        return allGuestsMyBookings;
    }

    /// <summary>
    /// get guest ids for anyone who has ever booked a room
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("Statistics/AllGuestIds")]

    public async Task<IEnumerable<string>> GetGuestIdsForThoseWhoHasEverBooked(CancellationToken cancellationToken)
    {
        //using extension in Eventuous.Projections.MongoDB.Tools
        var allGuestsMyBookings = await _mongoDb.LoadDocument<StatisticsOfBookingsDocument>("statistics", cancellationToken);
        return allGuestsMyBookings.Guests.Select(x => x.GuestId).ToList();
    }
}