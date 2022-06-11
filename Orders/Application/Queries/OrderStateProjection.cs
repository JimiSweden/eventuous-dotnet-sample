
using Eventuous.Projections.MongoDB;
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

            On<OrderEvents.V1.OrderRowAdded>(evt => evt.OrderId, HandleOrderRowAdded);
            On<OrderEvents.V1.OrderRowDeleted>(evt => evt.OrderId, HandleOrderRowDeleted);

            //note: this takes a filterDefinitionBuilder, needed to find the order-row inside the order-document.OrderRows
            On<OrderEvents.V1.OrderRowAmountChanged>(OrderRowMatchOnRowIdFilter, HandleOrderRowAmountChanged);



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

        /// <summary>
        /// Filter (definition) that matches the OrderRowId in Order.OrderRows, for operation on an OrderRow <br/>
        /// (note: For more info on filtering Arrays and a Lambda example of this method see "README resources tips n tricks.md" in root)
        /// </summary>
        private FilterDefinition<OrderDocument> OrderRowMatchOnRowIdFilter(OrderEvents.V1.OrderRowAmountChanged evt,
            FilterDefinitionBuilder<OrderDocument> filter)
        {
            //match main document on Id.
            var documentFilter = new ExpressionFilterDefinition<OrderDocument>(x => x.Id == evt.OrderId);
            
            //match array element by id. (notice the type is OrderRow here)
            var orderRowFilterDefinition = new FilterDefinitionBuilder<OrderDocument.OrderRow>().Eq(x => x.OrderRowId, evt.OrderRowId);
            //match field OrderRows, with the Equality filter for matching on Id (single in this case but could in theory be more than one row)
            var documentOrderRowsFilterDefinition = new FilterDefinitionBuilder<OrderDocument>().ElemMatch(x => x.OrderRows, orderRowFilterDefinition);
            //an AND filter is needed to first match the main document (Order) and then the array element (OrderRow)
            return filter.And(new List<FilterDefinition<OrderDocument>>
            {
                documentFilter,
                documentOrderRowsFilterDefinition
            });
        }


        /// <summary>
        /// Using "pull filter" - to remove element from array inside a document
        /// Note, this is used together with the Filter in <see cref="OrderRowMatchOnRowIdFilter"/> <br/>
        /// the array positional parameter '-1' (minus one) might look wrong, but it is translated to '$' for positioning in MongoDB <br/>
        /// (note: For more info on filtering Arrays see "README resources tips n tricks.md" in root)
        /// </summary>
        private UpdateDefinition<OrderDocument> HandleOrderRowAmountChanged(OrderEvents.V1.OrderRowAmountChanged evt,
            UpdateDefinitionBuilder<OrderDocument> update)
            => update.Set(x => x.OrderRows[-1].ProductAmount, evt.ProductAmount);
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        private UpdateDefinition<OrderDocument> HandleOrderRowDeleted(OrderEvents.V1.OrderRowDeleted evt, UpdateDefinitionBuilder<OrderDocument> update)
            => update.PullFilter(x => x.OrderRows, row => row.OrderRowId == evt.OrderRowId);


        private UpdateDefinition<OrderDocument> HandleOrderRowAdded(OrderEvents.V1.OrderRowAdded evt, UpdateDefinitionBuilder<OrderDocument> update)
            => update.AddToSet(x => x.OrderRows, new OrderDocument.OrderRow(
            evt.OrderRowId, evt.ProductId, evt.ProductName, evt.ProductType,
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
            => update.Set(x => x.Booked, true)
                .Set(x => x.OrderBookedDate, evt.OrderBookedDate)
                .Set(x => x.Paid, evt.OutstandingAmount == 0)
                .Set(x => x.Price, evt.OrderPrice)
                .Set(x => x.Currency, evt.Currency)
                .Set(x => x.Outstanding, evt.OutstandingAmount)
                .Set(x => x.PrepaidAmount, evt.PrepaidAmount)
                .Set(x => x.Discount, evt.DiscountAmount);
    }
}
