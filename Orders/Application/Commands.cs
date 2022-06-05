using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orders.Domain;
using Orders.HttpApi.Orders;

namespace Orders.Application
{
    /// <summary>
    /// Commands are what is coming in from your <see cref="CommandApi"/> <br/>
    /// They have what is needed for the intention of the command <br/>
    /// (And should not be confused with the events, OrderEvents, in the domain <br/>
    /// although they should match at least to some degree
    /// 
    /// </summary>
    public class OrderCommands
    {
        public record AddOrder(
            string OrderId,
            string CustomerId,
            DateTimeOffset OrderCreatedAt
        );


        public record BookOrder(
            string OrderId,
            float OrderPrice,
            float PrePaidAmount,
            string Currency,
            float DiscountAmount,
            //todo: shipping // and maybe invoice address.
            DateTimeOffset OrderBookedAt
        );


        public record RecordPayment(
            string OrderId, 
            float PaidAmount, 
            string Currency, 
            string PaymentId, 
            string PaidBy
        );

        public record UnBookOrder(
            string OrderId,
            string Reason,
            string UnBookedBy,
            DateTimeOffset OrderUnBookedAt
        );
        
        public record CancelOrder(
            string OrderId,
            string Reason,
            string CancelledBy,
            DateTimeOffset OrderCancelledAt
        );

        public record AddOrderRow(
            string OrderId,
            string OrderRowId, //same as productId
            string ProductId,
            int ProductAmount,
            //todo: 
            //following props would be fetched in Order.cs
            // from a product service.. 
            // but for now lets keep them here.
            string ProductName,
            string ProductType,
            string ProductDescription,
            float ProductPrice,
            string Currency
        );

    }
}
