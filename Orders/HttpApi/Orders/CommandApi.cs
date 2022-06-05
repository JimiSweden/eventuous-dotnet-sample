
using Eventuous;
using Eventuous.AspNetCore.Web;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization; //TODO..
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


        [HttpPost]
        [Route("cancel")]
        public Task<ActionResult<Result>> CancelOrder(
            [FromBody] OrderCommands.CancelOrder cmd,
            CancellationToken cancellationToken)
            => Handle(cmd, cancellationToken);



        [HttpPost]
        [Route("recordPayment")]
        public Task<ActionResult<Result>> RecordPayment(
            [FromBody] OrderCommands.RecordPayment cmd, 
            CancellationToken cancellationToken
        ) => Handle(cmd, cancellationToken);

    }
}
