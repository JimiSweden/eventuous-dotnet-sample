using Bookings.Domain.Bookings;
using Eventuous;
using Eventuous.AspNetCore.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Bookings.Application.BookingCommands;

namespace Bookings.HttpApi.Bookings;

[Route("/booking")]
public class CommandApi : CommandHttpApiBase<Booking> {
    public CommandApi(IApplicationService<Booking> service) : base(service) { }

    /// <summary>
    /// Adds a Booking (to the EventStoreDB)
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>BookingState</returns>
    [HttpPost]
    [Route("book")]
    public Task<ActionResult<Result>> BookRoom([FromBody] BookRoom cmd, CancellationToken cancellationToken)
        => Handle(cmd, cancellationToken);

   /// <summary>
   /// Update a booking; requires an existing booking (in EventStoreDB)
   /// </summary>
   /// <param name="cmd"></param>
   /// <param name="cancellationToken"></param>
   /// <returns></returns>
    [HttpPost]
    [Route("change")]
    public Task<ActionResult<Result>> ChangeBooking([FromBody] ChangeBooking cmd, CancellationToken cancellationToken)
        => Handle(cmd, cancellationToken);

    /// <summary>
    /// This endpoint is for demo purposes only. The normal flow to register booking payments is to submit
    /// a command via the Booking.Payments HTTP API. It then gets propagated to the Booking aggregate
    /// via the integration messaging flow.
    /// </summary>
    /// <param name="cmd">Command to register the payment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    [HttpPost]
    [Route("recordPayment")]
    public Task<ActionResult<Result>> RecordPayment(
        [FromBody] RecordPayment cmd, CancellationToken cancellationToken
    )
        => Handle(cmd, cancellationToken);
}
