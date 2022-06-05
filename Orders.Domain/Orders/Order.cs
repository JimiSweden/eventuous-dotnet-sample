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
    /// The Aggregate class is used for validating the commands
    /// from the CommandService (i.e. CommandHandler) <br/>
    ///
    /// Apply then Applies a new event to the state,
    /// adds the event to the list of pending changes,
    /// and increases the current version.
    /// </summary>
    public class Order : Aggregate<OrderState, OrderId>
    {
        /// <summary>
        /// TODO: OrderId should be created here, not from outside.
        /// {customerId}_{orderCreatedAt} / perhaps.. 
        /// ex: 12346_2022-06-04 193056 : where 193056 HHMMSS
        /// Possible problems with timestamp in Id
        /// is if a user creates multiple orders by mistake.
        ///
        /// TODO ? : Order should perhaps not contain logic for adding and removing rows
        /// and creating empty order etc..
        /// Instead a CustomerCart aggregate can contain that type of history
        /// - and when confirming everything in the cart, the order is created and booked
        /// at the same time. <br/>
        /// But, then again, altering the order rows might be a wanted function
        /// - 
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="customerId"></param>
        /// <param name="orderCreatedAt"></param>
        /// <returns></returns>
        public async Task AddOrder(
            OrderId orderId,
            string customerId,
            DateTimeOffset orderCreatedAt
        )
        {
            //from the Eventuous base class 'Aggregate'
            //checks that the CurrentVersion is not set (i.e. Aggregate exists)
            //will throw a 'DomainException' if it is
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
             *   ex: - not paid and invoicing not allowed,
        *        - or not paid and customer outstanding total exceeds credit limit
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



        public async Task CancelOrder(
            string reason,
            string cancelledBy,
            DateTimeOffset orderCancelledAt
        )
        {
            EnsureExists();

            if (!State.Booked)
            {
                throw new DomainException("Order is not Booked yet and thus can't be cancelled");
            }

            if (State.Cancelled)
            {
                throw new DomainException($"Order is already cancelled at; {State.OrderCancelledDate}, by: {State.CancelledBy}");
            }


            //todo: validate business rules if order can be cancelled.
            


            Apply(new OrderEvents.V1.OrderCancelled(
                State.Id,
                reason,
                cancelledBy,
                orderCancelledAt
            ));

        }




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

            if (State.Cancelled)
            {
                throw new DomainException("Order is canceled thus can't be cancelled");
            }

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
