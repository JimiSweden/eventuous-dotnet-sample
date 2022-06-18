﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eventuous;
using Orders.Domain;
using Orders.Domain.Orders;
using Orders.HttpApi.Orders;

namespace Orders.Application
{
    /// <summary>
    /// OrdersCommandService is registered in <see cref="Registrations"/> <br/>
    /// All order commands are executed via CommandHttpApiBase.Handle in the <see cref="CommandApi"/>
    /// </summary>
    public class OrdersCommandService : ApplicationService<Order, OrderState, OrderId>
    {
        public OrdersCommandService(IAggregateStore store) : base(store)
        {
            /* A note on the params to Async methods.
             * the third param '_' is the cancellation token
             * (ActOnAggregateAsync takes 3 params)
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

            OnExistingAsync<OrderCommands.UnBookOrder>(
                cmd => new OrderId(cmd.OrderId),
                (order, cmd, _) => order.UnBookOrder(
                    cmd.Reason,
                    cmd.UnBookedBy,
                    DateTimeOffset.Now
                )
            );

            OnExistingAsync<OrderCommands.CancelOrder>(
                cmd => new OrderId(cmd.OrderId),
                (order, cmd, _) => order.CancelOrder(
                    cmd.Reason,
                    cmd.CancelledBy,
                    DateTimeOffset.Now
                    )
                );


            OnExistingAsync<OrderCommands.AddOrderRow>(
                cmd => new OrderId(cmd.OrderId),
                (order, cmd, _) => order.AddOrderRow(
                    cmd.OrderRowId,
                    cmd.ProductId,
                    cmd.ProductAmount,
                    //todo: 
                    //following props would be fetched in Order.cs
                    // from a product service.. 
                    // but for now lets keep them here.
                    cmd.ProductName,
                    cmd.ProductType,
                    cmd.ProductDescription,
                    cmd.ProductPrice,
                    cmd.Currency,
                    //--<
                    DateTimeOffset.Now
                )
            );

            OnExistingAsync<OrderCommands.DeleteOrderRow>(
                cmd => new OrderId(cmd.OrderId),
                (order, cmd, _) => order.DeleteOrderRow(
                    cmd.OrderRowId,
                    cmd.ProductId
                )
            );

            OnExistingAsync<OrderCommands.UpdateOrderRowAmount>(
                cmd => new OrderId(cmd.OrderId),
                (order, cmd, _) => order.UpdateOrderRowAmount(
                    cmd.OrderRowId,
                    cmd.ProductAmount
                )
            );

            OnExistingAsync<OrderCommands.AddInvoiceAddress>(
                cmd => new OrderId(cmd.OrderId),
                (order, cmd, _) => order.AddInvoiceAddress(
                    cmd.Name,
                    cmd.CompanyName,
                    cmd.PhoneNumber,
                    cmd.EmailAddress,
                    cmd.StreetName,
                    cmd.StreetNumber,
                    cmd.ApartmentOrOfficeInfo,
                    cmd.Postcode,
                    cmd.PostTown,
                    cmd.Country
                )
            );

            OnExistingAsync<OrderCommands.AddShippingAddress>(
                cmd => new OrderId(cmd.OrderId),
                (order, cmd, _) => order.AddShippingAddress(
                    cmd.Name,
                    cmd.CompanyName,
                    cmd.PhoneNumber,
                    cmd.EmailAddress,
                    cmd.StreetName,
                    cmd.StreetNumber,
                    cmd.ApartmentOrOfficeInfo,
                    cmd.Postcode,
                    cmd.PostTown,
                    cmd.Country,
                    cmd.IsResidential
                )
            );
            OnExistingAsync<OrderCommands.RemoveShippingAddress>(
                cmd => new OrderId(cmd.OrderId),
                (order, cmd, _) => order.RemoveShippingAddress()
            );

        }
    }
    
}
