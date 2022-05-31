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

    [HttpPost]
    [Route("book")]
    public Task<ActionResult<Result>> BookRoom([FromBody] BookRoom cmd, CancellationToken cancellationToken)
        => Handle(cmd, cancellationToken);

    [HttpPost]
    [Route("change")]
    public Task<ActionResult<Result>> ChangeBooking([FromBody] ChangeBooking cmd, CancellationToken cancellationToken)
        => Handle(cmd, cancellationToken);


    [HttpPost]
    [Route("recordPayment")]
    public Task<ActionResult<Result>> RecordPayment(
        [FromBody] RecordPayment cmd, CancellationToken cancellationToken
    )
        => Handle(cmd, cancellationToken);
}
