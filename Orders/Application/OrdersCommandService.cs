using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eventuous;
using Orders.Domain;
using Orders.Domain.Orders;

namespace Orders.Application
{
    public class OrdersCommandService : ApplicationService<Order, OrderState, OrderId>
    {
        public OrdersCommandService(IAggregateStore store) : base(store)
        {
            /* A note on the params to Async methods.
             * the third param '_' is needed if we don't pass anything
             * since ActOnAggregateAsync takes 3 params
             * (cancellationToken is the last param)
             */

            OnNewAsync<OrderCommands.AddOrder>(
                (order, cmd, _) => order.AddOrder(
                    new OrderId(cmd.OrderId),
                    cmd.CustomerId,
                    DateTimeOffset.Now
                    )
                );

            /* OnExisting operates on the Id to get the existing order, 
             * then the "change" uses the State.Id internally
            */
            OnExistingAsync<OrderCommands.BookOrder>(
                cmd => new OrderId(cmd.OrderId),
                (order, cmd, _) => order.BookOrder(
                    new Money(cmd.OrderPrice, cmd.Currency),
                    new Money(cmd.PrePaidAmount, cmd.Currency),
                    new Money(cmd.DiscountAmount, cmd.Currency),
                    DateTimeOffset.Now
                        )
                );
        }
    }
    
}
