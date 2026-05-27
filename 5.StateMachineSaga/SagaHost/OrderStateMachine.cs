using Contracts;
using MassTransit;

namespace SagaHost;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    // ── Forward states ────────────────────────────────────────────────────────
    public State Submitted         { get; private set; } = null!;
    public State WaitingForPayment { get; private set; } = null!;  // human step
    public State Paid              { get; private set; } = null!;
    public State Shipped           { get; private set; } = null!;

    // ── Compensation states ───────────────────────────────────────────────────
    public State RefundingPayment   { get; private set; } = null!;
    public State ReleasingInventory { get; private set; } = null!;
    public State Failed             { get; private set; } = null!;

    public State Cancelled { get; private set; } = null!;

    // ── Forward events ────────────────────────────────────────────────────────
    public Event<SubmitOrder>      OrderSubmitted      { get; private set; } = null!;
    public Event<InventoryReserved> InventoryReservedEvt { get; private set; } = null!;
    public Event<PaymentReceived>   PaymentReceivedEvt   { get; private set; } = null!;  // human
    public Event<ShipmentCreated>   ShipmentCreatedEvt   { get; private set; } = null!;

    // ── Compensation events ───────────────────────────────────────────────────
    public Event<ShipmentFailed>    ShipmentFailedEvt    { get; private set; } = null!;
    public Event<PaymentRefunded>   PaymentRefundedEvt   { get; private set; } = null!;
    public Event<InventoryReleased> InventoryReleasedEvt { get; private set; } = null!;

    public Event<CancelOrder> OrderCancelled { get; private set; } = null!;

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderSubmitted,       x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => InventoryReservedEvt, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => PaymentReceivedEvt,   x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => ShipmentCreatedEvt,   x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => ShipmentFailedEvt,    x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => PaymentRefundedEvt,   x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => InventoryReleasedEvt, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => OrderCancelled,       x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        // ── FORWARD FLOW ──────────────────────────────────────────────────────

        Initially(
            When(OrderSubmitted)
                .Then(ctx =>
                {
                    ctx.Saga.CustomerName            = ctx.Message.CustomerName;
                    ctx.Saga.Amount                  = ctx.Message.Amount;
                    ctx.Saga.SimulateShippingFailure = ctx.Message.SimulateShippingFailure;
                    Console.WriteLine($"\n  [SAGA] SubmitOrder      → Submitted  | {ctx.Message.CustomerName} ${ctx.Message.Amount:F2}");
                    Console.WriteLine($"  [SAGA] ──► Sending ReserveInventory");
                })
                .TransitionTo(Submitted)
                .Publish(ctx => new ReserveInventory { CorrelationId = ctx.Saga.CorrelationId }));

        During(Submitted,
            When(InventoryReservedEvt)
                .Then(ctx =>
                {
                    Console.WriteLine($"  [SAGA] InventoryReserved → WaitingForPayment");
                    Console.WriteLine($"  [SAGA] ──► Sending payment link email");
                })
                .TransitionTo(WaitingForPayment)
                .Publish(ctx => new RequestPayment
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    CustomerName  = ctx.Saga.CustomerName,
                    Amount        = ctx.Saga.Amount,
                    PaymentLink   = $"https://pay.example.com/{ctx.Saga.CorrelationId}"
                }),
            When(OrderCancelled)
                .Then(ctx =>
                {
                    ctx.Saga.CancellationReason = ctx.Message.Reason;
                    Console.WriteLine($"  [SAGA] CancelOrder       → Cancelled  | {ctx.Message.Reason}");
                })
                .TransitionTo(Cancelled));

        // Saga nằm chờ ở đây — vô thời hạn — cho đến khi user trả tiền
        During(WaitingForPayment,
            When(PaymentReceivedEvt)
                .Then(ctx =>
                {
                    ctx.Saga.TransactionId = ctx.Message.TransactionId;
                    Console.WriteLine($"  [SAGA] PaymentReceived   → Paid       | TxId: {ctx.Message.TransactionId}");
                    Console.WriteLine($"  [SAGA] ──► Sending CreateShipment");
                })
                .TransitionTo(Paid)
                .Publish(ctx => new CreateShipment
                {
                    CorrelationId   = ctx.Saga.CorrelationId,
                    CustomerName    = ctx.Saga.CustomerName,
                    SimulateFailure = ctx.Saga.SimulateShippingFailure
                }),
            When(OrderCancelled)
                .Then(ctx =>
                {
                    ctx.Saga.CancellationReason = ctx.Message.Reason;
                    Console.WriteLine($"  [SAGA] CancelOrder       → Cancelled  | {ctx.Message.Reason}");
                })
                .TransitionTo(Cancelled));

        During(Paid,
            When(ShipmentCreatedEvt)
                .Then(ctx =>
                {
                    ctx.Saga.TrackingNumber = ctx.Message.TrackingNumber;
                    Console.WriteLine($"  [SAGA] ShipmentCreated   → Shipped    | Tracking: {ctx.Message.TrackingNumber}");
                    Console.WriteLine($"  [SAGA] ✓ Order complete!");
                })
                .TransitionTo(Shipped),

            // ── COMPENSATION ─────────────────────────────────────────────────
            When(ShipmentFailedEvt)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    Console.WriteLine($"  [SAGA] ShipmentFailed    → RefundingPayment | {ctx.Message.Reason}");
                    Console.WriteLine($"  [SAGA] ──► Sending RefundPayment (compensation 1/2)");
                })
                .TransitionTo(RefundingPayment)
                .Publish(ctx => new RefundPayment
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    Amount        = ctx.Saga.Amount,
                    TransactionId = ctx.Saga.TransactionId!
                }),
            When(OrderCancelled)
                .Then(ctx =>
                {
                    ctx.Saga.CancellationReason = ctx.Message.Reason;
                    Console.WriteLine($"  [SAGA] CancelOrder       → Cancelled  | {ctx.Message.Reason}");
                })
                .TransitionTo(Cancelled));

        During(RefundingPayment,
            When(PaymentRefundedEvt)
                .Then(ctx =>
                {
                    Console.WriteLine($"  [SAGA] PaymentRefunded   → ReleasingInventory");
                    Console.WriteLine($"  [SAGA] ──► Sending ReleaseInventory (compensation 2/2)");
                })
                .TransitionTo(ReleasingInventory)
                .Publish(ctx => new ReleaseInventory { CorrelationId = ctx.Saga.CorrelationId }));

        During(ReleasingInventory,
            When(InventoryReleasedEvt)
                .Then(ctx =>
                {
                    Console.WriteLine($"  [SAGA] InventoryReleased → Failed");
                    Console.WriteLine($"  [SAGA] ✗ Fully compensated. Reason: {ctx.Saga.FailureReason}");
                })
                .TransitionTo(Failed));
    }
}
