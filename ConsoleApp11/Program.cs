using Stateless;
using Stateless.Graph;
using System;

namespace OrderSaga
{
    public enum OrderStates
    {
        Submitted = 0,
        Confirmed = 1,
        Unconfirmed = 2,
        Reserving = 3,
        Reserved = 4,
        ReservationFailed = 5,
        PendingPayment = 6,
        PaymentFailed = 7,
        PaymentSucceeded = 8,
        TicketIssuing = 9,
        TicketIssuingFailed = 10,
        TicketIssued = 11
    }

    public enum OrderTriggers
    {
        Submitted = 0,
        Confirmed = 1,
        Unconfirmed = 2,
        ReservationRequested = 3,
        ReservationSucceeded = 4,
        ReservationFailed = 5,
        IssuanceInitiated = 6,
        Refunded = 7,
        IssuanceFailed = 8,
        Finished = 9,
        PaymentInitiated = 10,
        PaymentSucceeded = 11,
        PaymentFailed = 12,
        IssuanceSucceeded = 13
    }

    public class OrderStateMachine
    {
        private StateMachine<OrderStates, OrderTriggers> _machine;
        public OrderStates State { get; private set; } = OrderStates.Submitted;

        public OrderStateMachine(OrderStates state)
        {
            this.State = state;

            _machine = new StateMachine<OrderStates, OrderTriggers>(() => State, s => State = s);
            _machine.Configure(OrderStates.Submitted)
                .Permit(OrderTriggers.Confirmed, OrderStates.Confirmed)
                .Permit(OrderTriggers.Unconfirmed, OrderStates.Unconfirmed)
                .OnEntry(() => Print(OrderTriggers.Submitted, OrderStates.Submitted));

            _machine.Configure(OrderStates.Confirmed)
                .Permit(OrderTriggers.ReservationRequested, OrderStates.Reserving);

            _machine.Configure(OrderStates.Reserving)
                .Permit(OrderTriggers.ReservationSucceeded, OrderStates.Reserved)
                .Permit(OrderTriggers.ReservationFailed, OrderStates.ReservationFailed);

            _machine.Configure(OrderStates.Reserved)
                .Permit(OrderTriggers.PaymentInitiated, OrderStates.PendingPayment);

            _machine.Configure(OrderStates.PendingPayment)
                .Permit(OrderTriggers.PaymentSucceeded, OrderStates.PaymentSucceeded)
                .Permit(OrderTriggers.PaymentFailed, OrderStates.PaymentFailed);

            _machine.Configure(OrderStates.PaymentSucceeded)
                .Permit(OrderTriggers.IssuanceInitiated, OrderStates.TicketIssuing);

            _machine.Configure(OrderStates.TicketIssuing)
               .Permit(OrderTriggers.IssuanceFailed, OrderStates.TicketIssuingFailed)
               .Permit(OrderTriggers.IssuanceSucceeded, OrderStates.TicketIssued);
        }

        private static void Print(OrderTriggers trigger, OrderStates state)
        {
            Console.WriteLine($"Trigger: {trigger}, State: {state}");
        }

        public string GetGraph() => UmlDotGraph.Format(_machine.GetInfo());
    }

    public static class Program
    {
        static void Main(string[] _)
        {
            var machine = new OrderStateMachine(OrderStates.PendingPayment);
            Console.WriteLine(machine.GetGraph());
        }
    }
}