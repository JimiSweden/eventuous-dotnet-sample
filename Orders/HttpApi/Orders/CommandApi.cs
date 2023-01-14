
using Eventuous;
using Eventuous.AspNetCore.Web;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization; //TODO..
// read https://eventuous.dev/docs/application/command-api/
// "Then, you can use the HttpContext data in your command:"
using Orders.Application;
using Orders.Domain.Orders;

namespace Orders.HttpApi.Orders
{
    /// <summary>
    /// this command api operates on a (one) Order,
    /// </summary>
    [Route("/order")]
    public class CommandApi : CommandHttpApiBase<Order>
    {
        public CommandApi(IApplicationService<Order> service)
        :base(service)
        { }

        [HttpPost]
        [Route("add")]
        public Task<ActionResult<Result>> AddOrder(
            [FromBody] OrderCommands.AddOrder cmd,
            CancellationToken cancellationToken)
        {
            try
            {
                return Handle(cmd, cancellationToken);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
            
                                              
        [HttpPost]
        [Route("book")]
        public Task<ActionResult<Result>> BookOrder(
            [FromBody] OrderCommands.BookOrder cmd,
            CancellationToken cancellationToken)
            => Handle(cmd, cancellationToken);

        /// <summary>
        /// unbook, i.e. reopen order for editing after it has been booked
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("unbook")]
        public Task<ActionResult<Result>> UnBookOrder(
            [FromBody] OrderCommands.UnBookOrder cmd,
            CancellationToken cancellationToken)
            => Handle(cmd, cancellationToken);


        [HttpPost]
        [Route("cancel")]
        public Task<ActionResult<Result>> CancelOrder(
            [FromBody] OrderCommands.CancelOrder cmd,
            CancellationToken cancellationToken)
            => Handle(cmd, cancellationToken);



        [HttpPost]
        [Route("row/add")]
        public Task<ActionResult<Result>> AddOrderRow(
            [FromBody] OrderCommands.AddOrderRow cmd,
            CancellationToken cancellationToken)
            => Handle(cmd, cancellationToken);

        [HttpPost]
        [Route("row/remove")]
        public Task<ActionResult<Result>> DeleteOrderRow(
            [FromBody] OrderCommands.DeleteOrderRow cmd,
            CancellationToken cancellationToken)
            => Handle(cmd, cancellationToken);
        
        [HttpPost]
        [Route("row/updateamount")]
        public Task<ActionResult<Result>> UpdateOrderRowAmount(
            [FromBody] OrderCommands.UpdateOrderRowAmount cmd,
            CancellationToken cancellationToken)
            => Handle(cmd, cancellationToken);



        [HttpPost]
        [Route("recordPayment")]
        public Task<ActionResult<Result>> RecordPayment(
            [FromBody] OrderCommands.RecordPayment cmd, 
            CancellationToken cancellationToken
        ) => Handle(cmd, cancellationToken);


        [HttpPost]
        [Route("address/invoice/add")]
        public Task<ActionResult<Result>> AddInvoiceAddress(
            [FromBody] OrderCommands.AddInvoiceAddress cmd,
            CancellationToken cancellationToken)
            => Handle(cmd, cancellationToken);

        [HttpPost]
        [Route("address/shipping/add")]
        public Task<ActionResult<Result>> AddShippingAddress(
            [FromBody] OrderCommands.AddShippingAddress cmd,
            CancellationToken cancellationToken)
            => Handle(cmd, cancellationToken);[HttpPost]
        
        [Route("address/shipping/remove")]
        public Task<ActionResult<Result>> RemoveShippingAddress(
            [FromBody] OrderCommands.RemoveShippingAddress cmd,
            CancellationToken cancellationToken)
            => Handle(cmd, cancellationToken);
    }
}
