using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Eventuous;
using NodaTime;

namespace Orders.Domain.Orders
{
    public static class OrderEvents
    {
        public static class V1
        {
            [EventType("V1.OrderAdded")] // https://eventuous.dev/docs/persistence/serialisation/#auto-registration-of-types
            public record OrderAdded(
                string OrderId, 
                string CustomerId,
                DateTimeOffset OrderCreatedDate
                    // IEnumerable<string> orderRowIds,
                    //float OrderPrice,
                    //float PrepaidAmount,
                    //float DiscountAmount,
                    //float OutstandingAmount,
                    //string Currency
                    );

            [EventType("V1.OrderBooked")]
            public record OrderBooked(
                string OrderId,
                //string customerId,
                DateTimeOffset OrderBookedDate,
                // IEnumerable<string> orderRowIds,
                float OrderPrice,
                float PrepaidAmount,
                float DiscountAmount,
                float OutstandingAmount,
                string Currency
            );

            [EventType("V1.OrderRowAdded")]
            public record OrderRowAdded(
                string OrderId,
                string OrderRowId,
                string ProductId,
                int ProductAmount
                );

            [EventType("V1.OrderRowDeleted")]
            public record OrderRowDeleted(
                string OrderId,
                string OrderRowId,
                string ProductId,
                int ProductAmount
                );

            //i.e. product added / removed from row 
            [EventType("V1.OrderRowAmountChanged")]
            public record OrderRowAmountChanged(
                string OrderId,
                string OrderRowId,
                int ProductAmount
                );


            [EventType("V1.PaymentRecorded")]
            public record PaymentRecorded(
                string OrderId,
                float PaidAmount,
                float Outstanding,
                string Currency,
                string PaymentId,
                string PaidBy,
                DateTimeOffset PaidAt
                );

            [EventType("V1.FullyPaid")]
            public record OrderFullyPaid(string OrderId, DateTimeOffset FullyPaidAt);

            [EventType("V1.OrderCancelled")]
            public record OrderCancelled(
                OrderId OrderId, 
                string Reason, 
                string CancelledBy, 
                DateTimeOffset OrderCancelledAt
                );

        }

    }
}
