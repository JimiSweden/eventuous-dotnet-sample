using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eventuous.Projections.MongoDB;
using Eventuous.Projections.MongoDB.Tools;
using MongoDB.Driver;
using Orders.Domain.Orders;

namespace Orders.Application.Queries
{
    public class OrderStateProjection : MongoProjection<OrderDocument>
    {
        public OrderStateProjection(IMongoDatabase database) : base(database)
        {

            On<OrderEvents.V1.OrderAdded>(evt => evt.OrderId, HandleOrderAdded);
            On<OrderEvents.V1.OrderBooked>(evt => evt.OrderId, HandleOrderBooked);
            On<OrderEvents.V1.OrderCancelled>(evt => evt.OrderId, HandleOrderCancelled);
            On<OrderEvents.V1.OrderCancelled>(evt => evt.OrderId, HandleOrderCancelled);
            On<OrderEvents.V1.OrderRowAdded>(evt => evt.OrderId, HandleOrderRowAdded);
            On<OrderEvents.V1.OrderRowDeleted>(evt => evt.OrderId, HandleOrderRowDeleted);
            
            //TODO: order-rows : Add/remove/update


            On<OrderEvents.V1.PaymentRecorded>(
                evt => evt.OrderId,
                (evt, update) => update
                    .Set(x => x.Outstanding, evt.Outstanding)
            );
            On<OrderEvents.V1.OrderFullyPaid>(
                evt => evt.OrderId,
                (_, update) => update.Set(x => x.Paid, true)
            );
        }

        private UpdateDefinition<OrderDocument> HandleOrderRowDeleted(OrderEvents.V1.OrderRowDeleted evt, UpdateDefinitionBuilder<OrderDocument> update)
            => update.PullFilter(x => x.OrderRows, row => row.OrderRowId == evt.OrderRowId);
        
        private UpdateDefinition<OrderDocument> HandleOrderRowAdded(OrderEvents.V1.OrderRowAdded evt, UpdateDefinitionBuilder<OrderDocument> update)
        
        => update.AddToSet(x => x.OrderRows, new OrderDocument.OrderRow(
            evt.OrderRowId, evt.ProductId, evt.ProductName,evt.ProductType,
            evt.ProductAmount));
        

        private UpdateDefinition<OrderDocument> HandleOrderCancelled(OrderEvents.V1.OrderCancelled evt,
            UpdateDefinitionBuilder<OrderDocument> update)
            => update.Set(x => x.Cancelled, true)
                .Set(x => x.CancelledBy, evt.CancelledBy)
                .Set(x => x.CancelledReason, evt.Reason)
                .Set(x => x.OrderCancelledDate, evt.OrderCancelledAt);


        private UpdateDefinition<OrderDocument> HandleOrderAdded(OrderEvents.V1.OrderAdded evt, UpdateDefinitionBuilder<OrderDocument> update)
            => update.SetOnInsert(x => x.Id, evt.OrderId)
                .Set(x => x.CustomerId, evt.CustomerId)
                .Set(x => x.OrderCreatedDate, evt.OrderCreatedDate);

        private UpdateDefinition<OrderDocument> HandleOrderBooked(OrderEvents.V1.OrderBooked evt, UpdateDefinitionBuilder<OrderDocument> update)
            => update.Set(x => x.Currency, evt.Currency)
                .Set(x => x.OrderBookedDate, evt.OrderBookedDate)
                .Set(x => x.Price, evt.OrderPrice)
                .Set(x => x.Outstanding, evt.OutstandingAmount)
                .Set(x => x.PrepaidAmount, evt.PrepaidAmount)
                .Set(x => x.Discount, evt.DiscountAmount);
    }

    public record OrderDocument : ProjectedDocument
    {
        public OrderDocument(string id) : base(id) { }
        public string CustomerId { get; set; }

        public string Currency { get; init; }
        public float Price { get; init; }
        public float Outstanding { get; init; }
        public float Discount { get; init; }
        public bool Paid { get; init; }
        public float PrepaidAmount { get; set; }


        /// <summary>
        /// when initial order was created
        /// </summary>
        public DateTimeOffset OrderCreatedDate { get; set; }

        /// <summary>
        /// When order was submitted (as finished/book) from customer
        /// </summary>
        public DateTimeOffset OrderBookedDate { get; set; }
        /// <summary>
        /// if not booked, it is still open for changes (like a in a customer shopping-cart)
        /// </summary>
        public bool Booked { get; init; }

        public bool Cancelled { get; init; }
        public DateTimeOffset OrderCancelledDate { get; set; }
        public string CancelledBy { get; set; }
        public string CancelledReason { get; set; }

        public List<OrderRow> OrderRows { get; set; } = new ();

        public record OrderRow (string OrderRowId, string ProductId, 
            string ProductName, 
            string ProductType,
            int ProductAmount);
    }
}
