using Contracts;
using MassTransit;

namespace Activities;

public class ShipOrderActivity : IActivity<OrderArguments, OrderLog>
{
    public async Task<ExecutionResult> Execute(ExecuteContext<OrderArguments> context)
    {
        var args = context.Arguments;
        var tracking = $"TRACK-{args.OrderId}-{DateTime.UtcNow:mmss}";

        Console.WriteLine($"  [DONE]       ShipOrder      — OrderId={args.OrderId}  Tracking={tracking}");

        return context.Completed(new OrderLog
        {
            OrderId     = args.OrderId,
            Step        = "ShipOrder",
            CompletedAt = DateTimeOffset.UtcNow
        });
    }

    public async Task<CompensationResult> Compensate(CompensateContext<OrderLog> context)
    {
        Console.WriteLine($"  [COMPENSATE] ShipOrder      — cancelling shipment for OrderId={context.Log.OrderId}");
        return context.Compensated();
    }
}
