using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eventuous;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Orders.Domain;
using Orders.Domain.Orders;

namespace Orders.HttpApi.Orders
{
    [Route("/orders")]
    public class QueryApi : ControllerBase
    {
        //EventStore
        private readonly IAggregateStore _store;

        //private readonly IMongoDatabase _mongoDb;
        /*
        * Read this for mongo db operations from dotnet
        * https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-6.0&tabs=visual-studio#add-a-crud-operations-service
        */

        public QueryApi(IAggregateStore store)
        {
            _store = store;
        }

        /// <summary>
        /// This reads from the AggregateStore, that is the EventStoreDB in our case (default)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<OrderState> GetOrder(string id, CancellationToken cancellationToken)
        {

            try
            {
                var order = await _store.Load<Order>(new OrderId(id), cancellationToken);

                //State returns the current aggregate state, i.e. latest data.
                return order.State;

            }
            catch (Eventuous.AggregateNotFoundException e) when(e.Message.Contains("not found."))
            {
                //todo: logging...
                Debugger.Break();
                throw;
            }

        }

    }
}
