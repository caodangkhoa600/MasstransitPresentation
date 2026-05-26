using System.Text.Json;
using Contract;
using MassTransit;

namespace Consumer;

public class OrderEventConsumer : IConsumer<DomainEvent>
{
    public Task Consume(ConsumeContext<DomainEvent> context)
    {
        var actionType = context.ReceiveContext.TransportHeaders.Get<string>("ActionType");

        if (string.IsNullOrEmpty(actionType))
        {
            Console.WriteLine("[WARN] Message has no ActionType header — skipping.");
            return Task.CompletedTask;
        }

        var payload = context.Message.Payload;

        Console.WriteLine($"\n[RECEIVED] ActionType={actionType}");

        switch (actionType)
        {
            case ActionTypes.OrderPlaced:
                var placed = payload.Deserialize<OrderPlacedMessage>();
                Console.WriteLine($"  → Order PLACED   | Id: {placed!.OrderId} | Customer: {placed.CustomerName} | Amount: {placed.Amount:C}");
                break;

            case ActionTypes.OrderCancelled:
                var cancelled = payload.Deserialize<OrderCancelledMessage>();
                Console.WriteLine($"  → Order CANCELLED | Id: {cancelled!.OrderId} | Reason: {cancelled.Reason}");
                break;

            case ActionTypes.OrderShipped:
                var shipped = payload.Deserialize<OrderShippedMessage>();
                Console.WriteLine($"  → Order SHIPPED   | Id: {shipped!.OrderId} | Tracking: {shipped.TrackingNumber}");
                break;

            default:
                Console.WriteLine($"  [WARN] Unsupported ActionType: {actionType}");
                break;
        }

        return Task.CompletedTask;
    }
}
