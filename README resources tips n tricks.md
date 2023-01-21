# MongoDb Driver

https://mongodb.github.io/mongo-csharp-driver/2.15/getting_started/quick_tour/

## pull filter - to remove element from array inside a document
https://stackoverflow.com/questions/30141958/mongodb-net-driver-2-0-pull-remove-element




# Updating arrays in mongodb with C#
A very good reference. https://kevsoft.net/2020/03/23/updating-arrays-in-mongodb-with-csharp.html

For implementation with Eventuous Projections see OrderStateProjection.cs

## Udate an array object, where the array is inside a document.

Orders.Application.Queries > OrderStateProjection.cs
```csharp
        /*
         * https://kevsoft.net/2020/03/23/updating-arrays-in-mongodb-with-csharp.html
         * Example using Builders..  and mongo driver 
         * var filter = Builders<Member>.Filter.Eq(x => x.Id, 1)
           & Builders<Member>.Filter.ElemMatch(x => x.Friends, Builders<Friend>.Filter.Eq(x => x.Id, 3));
           
           var update = Builders<Member>.Update.Set(x => x.Friends[-1].Name, "Bob");
           
           await db.members.UpdateOneAsync(filter, update);
         */

        /// <summary>
        /// Filter (definition) that matches the OrderRowId in Order.OrderRows, for operation on an OrderRow
        /// </summary>
        private FilterDefinition<OrderDocument> OrderRowMatchOnRowIdFilter(OrderEvents.V1.OrderRowAmountChanged evt,
            FilterDefinitionBuilder<OrderDocument> filter)
        {
            //match main document (Order) on Id.
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
        
        //...//


        /// <summary>
        /// same as above Filter but in lambda, perhaps cleaner but not as declerative.
        /// Filter that matches the OrderRowId in Order.OrderRows
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private FilterDefinition<OrderDocument> OrderRowMatchOnRowIdFilterLambdaStyle(OrderEvents.V1.OrderRowAmountChanged evt,
            FilterDefinitionBuilder<OrderDocument> filter)
            //an AND filter is needed to first match the main document (Order) and then the array element (OrderRow)
            => filter.And(new List<FilterDefinition<OrderDocument>>
            {
                new ExpressionFilterDefinition<OrderDocument>(x => x.Id == evt.OrderId),
                new FilterDefinitionBuilder<OrderDocument>().ElemMatch(x => x.OrderRows,
                    new FilterDefinitionBuilder<OrderDocument.OrderRow>().Eq(x => x.OrderRowId, evt.OrderRowId)
                    )
            });

```

