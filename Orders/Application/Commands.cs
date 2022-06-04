using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orders.Domain;

namespace Orders.Application
{
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


        public record RecordPayment(string OrderId, float PaidAmount, string Currency, string PaymentId, string PaidBy);
    }
}
