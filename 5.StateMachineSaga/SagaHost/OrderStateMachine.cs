using Contracts;
using MassTransit;

namespace SagaHost;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State Submitted { get; private set; } = null!;
    public State Accepted  { get; private set; } = null!;
    public State Shipped   { get; private set; } = null!;
    public State Cancelled { get; private set; } = null!;

    public Event<SubmitOrder> OrderSubmitted { get; private set; } = null!;
    public Event<AcceptOrder> OrderAccepted  { get; private set; } = null!;
    public Event<ShipOrder>   OrderShipped   { get; private set; } = null!;
    public Event<CancelOrder> OrderCancelled { get; private set; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderSubmitted, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => OrderAccepted,  x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => OrderShipped,   x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => OrderCancelled, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        Initially(
            When(OrderSubmitted)
                .Then(ctx =>
                {
                    ctx.Saga.CustomerName = ctx.Message.CustomerName;
                    ctx.Saga.Amount       = ctx.Message.Amount;
                    Console.WriteLine($"  [SAGA] SubmitOrder  → State: Submitted  | Customer: {ctx.Message.CustomerName} | Amount: ${ctx.Message.Amount:F2}");
                })
                .TransitionTo(Submitted));

        During(Submitted,
            When(OrderAccepted)
                .Then(ctx => Console.WriteLine($"  [SAGA] AcceptOrder  → State: Accepted"))
                .TransitionTo(Accepted),
            When(OrderCancelled)
                .Then(ctx =>
                {
                    ctx.Saga.CancellationReason = ctx.Message.Reason;
                    Console.WriteLine($"  [SAGA] CancelOrder  → State: Cancelled  | Reason: {ctx.Message.Reason}");
                })
                .TransitionTo(Cancelled));

        During(Accepted,
            When(OrderShipped)
                .Then(ctx =>
                {
                    ctx.Saga.TrackingNumber = ctx.Message.TrackingNumber;
                    Console.WriteLine($"  [SAGA] ShipOrder    → State: Shipped    | Tracking: {ctx.Message.TrackingNumber}");
                })
                .TransitionTo(Shipped),
            When(OrderCancelled)
                .Then(ctx =>
                {
                    ctx.Saga.CancellationReason = ctx.Message.Reason;
                    Console.WriteLine($"  [SAGA] CancelOrder  → State: Cancelled  | Reason: {ctx.Message.Reason}");
                })
                .TransitionTo(Cancelled));
    }
}
