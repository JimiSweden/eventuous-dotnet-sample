using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Eventuous;

namespace Orders.Domain.Orders
{

    /// <summary>
    /// The Aggregate class is used for validating the commands from the CommandService
    ///
    /// Apply then Applies a new event to the state,
    /// adds the event to the list of pending changes,
    /// and increases the current version.
    /// </summary>
    public class Order : Aggregate<OrderState, OrderId>
    {
        public async Task AddOrder(
            OrderId orderId,
            string customerId,
            DateTimeOffset orderCreatedAt
        )
        {
            //from the Eventuous base class 'Aggregate'
            //and checks that the CurrentVersion is not set,
            //will throw a 'DomainException' if it does
            EnsureDoesntExist();

            try
            {
                //from the Eventuous base class 'Aggregate'
                // will AddChange to internal list of pending changes,
                // bump the Current version, and return (previousState, newState)
                Apply(new OrderEvents.V1.OrderAdded(
                        orderId,
                        customerId,
                        orderCreatedAt
                        
                    )
                );
            }
            catch (Exception e)
            {
                Debugger.Break();
                throw;
            }
            
        }

        public async Task BookOrder(
            Money price,
            Money prepaid,
            Money discount,
           // ShippingAddress shippingAddress, //TODO.
            DateTimeOffset orderBookedAt
            )
        {
            //from the Eventuous base class 'Aggregate'
            //checks that the CurrentVersion is set (meaning the Order aggregate exists in store),
            //will throw a 'DomainException' if it does Not
            EnsureExists();

            //Note: since price and prepaid are records, the '-' operator is a method in Money,
            // containing business logic, i.e. validaion: (you can't subtract on different currencies)
            var outstanding = price - prepaid;


            if (State.Booked)
            {
                throw new DomainException("Order is already Booked");
            }

            /* todo: validate
             *  [ ] that the order has orderRows
             *  [ ] order is either payed or customer is allowed to be invoiced
             *
             */

            //from the Eventuous base class 'Aggregate'
            // will AddChange to internal list of pending changes,
            // bump the Current version, and return (previousState, newState)
            Apply(new OrderEvents.V1.OrderBooked(
                State.Id,
                orderBookedAt,
                price.Amount,
                prepaid.Amount,
                discount.Amount,
                outstanding.Amount,
                price.Currency
            ));

        }


        //todo: unbook / unlock /open for edit.
        //todo: cancel

        public void RecordPayment(
            Money paid,
            string paymentId,
            string paidBy,
            DateTimeOffset paidAt
        )
        {
            //from the Eventuous base class 'Aggregate'
            //and checks that the CurrentVersion is set,
            //will throw a 'DomainException' if it does Not.
            EnsureExists();

            if (State.HasPaymentBeenRecorded(paymentId)) return;

            var outstanding = State.Outstanding - paid;

            Apply(
                new OrderEvents.V1.PaymentRecorded(
                    State.Id,
                    paid.Amount,
                    outstanding.Amount,
                    paid.Currency,
                    paymentId,
                    paidBy,
                    paidAt
                )
            );

            MarkFullyPaidIfNecessary(paidAt);
        }

        void MarkFullyPaidIfNecessary(DateTimeOffset when)
        {
            if (State.Outstanding.Amount != 0) return;

            Apply(new OrderEvents.V1.OrderFullyPaid(State.Id, when));
        }
    }
}
