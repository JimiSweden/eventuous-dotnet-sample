using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eventuous.Projections.MongoDB.Tools;

namespace Orders.Application.Queries
{
    public record MyOrders : ProjectedDocument
    {
        /// <summary>
        /// where Id will be  CustomerId
        /// </summary>
        /// <param name="id"></param>
        public MyOrders(string id): base(id) { }

        public List<Order> Orders { get; set; }
        //public List<OrderBooked> OrdersBooked { get; set; }
        
        public DateTimeOffset OrderCreatedDate { get; set; }

        public record Order(string OrderId,
            //string CustomerId,
            DateTimeOffset OrderCreatedDate
        );

        //public record OrderBooked(string OrderId,
        //    string CustomerId,
        //    DateTimeOffset OrderBookedDate,
        //    bool Booked = true
        //);
    }
}
