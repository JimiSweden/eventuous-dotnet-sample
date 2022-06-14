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
                //string bookedBy, //todo
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
                string OrderRowId, //same as productId
                string ProductId,
                int ProductAmount,
                string ProductName,
                string ProductType,
                string ProductDescription,
                float ProductPrice,
                string Currency
                );

            [EventType("V1.OrderRowDeleted")]
            public record OrderRowDeleted(
                string OrderId,
                string OrderRowId,
                string ProductId
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


            [EventType("V1.OrderUnBooked")]
            public record OrderUnBooked(
                OrderId OrderId,
                string Reason,
                string UnBookedBy,
                DateTimeOffset OrderUnBookedAt
            );

            //https://en.wikipedia.org/wiki/Address#Format_by_country_and_area
            [EventType("V1.InvoiceAddressAdded")]
            public record InvoiceAddressAdded(
                OrderId OrderId,
                string Name,
                string Company,
                string PhoneNumber,
                string EmailAddress,
                string StreetName,
                string StreetNumber,
                string ApartmentOrOfficeInfo, //floor, apartment number, etc.
                string Postcode,
                string PostTown,
                string Country
            );

            [EventType("V1.ShippingAddressAdded")]
            public record ShippingAddressAdded(
                OrderId OrderId,
                string Name,
                string Company,
                string PhoneNumber,
                string EmailAddress,
                string StreetName,
                string StreetNumber,
                string ApartmentOrOfficeInfo, //floor, apartment number, etc.
                string Postcode,
                string PostTown,
                string Country, 
                bool? IsResidential
                );

            
            [EventType("V1.ShippingAddressRemoved")]
            public record ShippingAddressRemoved(OrderId OrderId);
        }

    }
}
