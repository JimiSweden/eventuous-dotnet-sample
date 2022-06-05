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

        /*
         * If you want to read "collections" from eventstore (not using mongo db projections)
         * you can create a subscription with filter as described here.
         * https://developers.eventstore.com/clients/grpc/subscriptions.html#filtering-by-event-type
         *
         * here is an example from https://discuss.eventstore.com/t/read-all-events-of-stream-type/3368/3
         * using the Category strem '$ce'
         *     var events = eventStoreClient.ReadStreamAsync(Direction.Forwards, "$ce-TechnologyGroup", StreamPosition.Start, resolveLinkTos: true);
           await foreach (var @event in events)
           {
           var eventType = Type.GetType($"{@event.Event.EventType}");
           
           IDomainEvent domainEvent = (IDomainEvent)JsonConvert.DeserializeObject(
           Encoding.UTF8.GetString(@event.Event.Data.ToArray()),
           eventType);
           }
         *
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


        /// <summary>
        /// This reads from the AggregateStore, that is the EventStoreDB in our case (default)
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        //[HttpGet]
        //public async Task<OrderState> GetOrders(CancellationToken cancellationToken)
        //{

        //    //try
        //    //{
        //    //    var order = await _store. <List<Order>>(cancellationToken);

        //    //    //State returns the current aggregate state, i.e. latest data.
        //    //    return order.State;

        //    //}
        //    //catch (Eventuous.AggregateNotFoundException e) when (e.Message.Contains("not found."))
        //    //{
        //    //    //todo: logging...
        //    //    Debugger.Break();
        //    //    throw;
        //    //}

        //}

    }
}
