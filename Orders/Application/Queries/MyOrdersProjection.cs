using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eventuous.Projections.MongoDB;
using Eventuous.Subscriptions.Context;
using MongoDB.Driver;
using Orders.Domain.Orders;

namespace Orders.Application.Queries
{
    /// <summary>
    /// This updates the projection holding "MyOrders" in MongoDb <br/>
    /// - adds the new order from the OrderAdded etc event to the current customers list of Orders <br/>
    /// </summary>
    public class MyOrdersProjection : MongoProjection<MyOrders>
    {
        public MyOrdersProjection(IMongoDatabase database): base(database)
        {
            //CustomerId is added to MyOrders as the collection-Id
            On<OrderEvents.V1.OrderAdded>(evt => evt.CustomerId, HandleOrderAdded);
        
            //todo: add customer id on event ? or skip listing booked here.. 
            //On<OrderEvents.V1.OrderBooked>(evt => evt.CustomerId, HandleOrderBooked);
            
        }

        //private UpdateDefinition<MyOrders> HandleOrderBooked(OrderEvents.V1.OrderBooked evt, 
        //    UpdateDefinitionBuilder<MyOrders> update)
        //{
        //    update.AddToSet(x => x.OrdersBooked,
        //        new MyOrders.OrderBooked(
        //            (evt.OrderId, evt.CustomerId, OrderBookedDate: default, Booked: false)
        //            );
        //}

        private UpdateDefinition<MyOrders> HandleOrderAdded(OrderEvents.V1.OrderAdded evt,
            UpdateDefinitionBuilder<MyOrders> update)
            => update.AddToSet(x => x.Orders,
                new MyOrders.Order
                    (evt.OrderId, /*evt.CustomerId,*/ evt.OrderCreatedDate)
                );

    }
    
}
