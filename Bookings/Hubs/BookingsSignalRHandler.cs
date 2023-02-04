using Eventuous.Subscriptions.Context;
using static Bookings.Domain.Bookings.BookingEvents;
using EventHandler = Eventuous.Subscriptions.EventHandler;

namespace Bookings.Hubs;

/// <summary>
/// Important, don't use this without filtering out old events,
/// Preferably us it in a subscription where you subscribe from "now".
/// </summary>
public class BookingsSignalRHandler : EventHandler
{
    private readonly IBookingsHubService _bookingsHubService;
    
    public BookingsSignalRHandler(IBookingsHubService bookingsHubService)
    {
        _bookingsHubService = bookingsHubService;
    
        //On<BookingPaymentRecorded>(async ctx => await HandlePayment(ctx.Message, ctx.CancellationToken));

        On<V1.RoomBooked>(async ctx => await HandleRoomBooked(ctx, ctx.CancellationToken));

        On<V1.BookingChanged>(async ctx => await HandleBookingChanged(ctx, ctx.CancellationToken));
        
        On<V1.BookingCancelled>(async ctx => await HandleBookingCancelled(ctx, ctx.CancellationToken));

    }

    private async Task HandleBookingCancelled(MessageConsumeContext<V1.BookingCancelled> consumeContextMessage, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();

    }

    private async Task HandleBookingChanged(MessageConsumeContext<V1.BookingChanged> ctx, CancellationToken cancellationToken)
    {
        await _bookingsHubService.BookingChanged(ctx.Stream.GetId(), ctx.Message);
    }

    private async Task HandleRoomBooked(MessageConsumeContext<V1.RoomBooked> ctx, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}