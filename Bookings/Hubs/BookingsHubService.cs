using System.Diagnostics;
using Bookings.Domain.Bookings;
using Microsoft.AspNetCore.SignalR;

namespace Bookings.Hubs;

public interface IBookingsHubService //: IBookingsHub
{

    public Task RoomBooked(string bookingId);
    public Task BookingChanged(string bookingid, BookingEvents.V1.BookingChanged bookingChanged);
    //    //bookingCancelled

}
/// <summary>
/// https://learn.microsoft.com/en-us/aspnet/core/signalr/hubcontext?view=aspnetcore-7.0#inject-an-instance-of-ihubcontext-in-a-controller
/// Important : Api Design Guidelines, object as param  > : https://learn.microsoft.com/en-us/aspnet/core/signalr/api-design?view=aspnetcore-7.0
/// blogpost on RPC and SignalR  https://www.tpeczek.com/2019/03/server-to-client-rpc-calls-with-results.html
/// </summary>
public class BookingsHubService : IBookingsHubService
{
    private readonly IHubContext<BookingsHub> _hubContext;
    
    /// <summary>
    /// Note:  When client methods are called from outside of the Hub class,
    /// there's no caller associated with the invocation.
    /// Therefore, there's no access to the ConnectionId, Caller, and Others properties.
    /// (using Groups and "Personal Groups" solves parts of this) 
    /// </summary>
    /// <param name="hubContext"></param>
    public BookingsHubService(IHubContext<BookingsHub> hubContext)
    {
        _hubContext = hubContext;
    }
    
    public async Task RoomBooked(string bookingId)
    {
        //await _hubContext.Clients.All.RoomBooked(bookingId);
        await _hubContext.Clients.All.SendAsync(HubConnectionMethods.UiClientMethods.RoomBooked, bookingId);

    }

    public async Task BookingChanged(string bookingId, BookingEvents.V1.BookingChanged bookingChanged)
    {
        try
        {
            var message = new
            {
                Title = "Room reserved",
                bookingChanged.RoomId,
                bookingChanged.CheckInDate,
                bookingChanged.CheckOutDate,
            };
            await _hubContext.Clients.All.SendAsync("bookingChanged", message);
            //await _hubContext.Clients.Group("AllLoggedIn").SendAsync("LoggedIn", "testmeddelande from service");

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Debugger.Break();
            throw;
        }
    }
    
    //bookingCancelled


    //public async Task RoomBooked(string bookingId)
    //{
    //    await _hub.Clients.All.RoomBooked(bookingId);
    //}

    //public async Task BookingChanged(string bookingid)
    //{
    //    await _hub.Clients.All.BookingChanged(bookingid);
    //}
}