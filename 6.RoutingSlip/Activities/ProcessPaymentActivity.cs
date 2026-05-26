using Contracts;
using MassTransit;

namespace Activities;

public class ProcessPaymentActivity : IActivity<OrderArguments, OrderLog>
{
    public async Task<ExecutionResult> Execute(ExecuteContext<OrderArguments> context)
    {
        var args = context.Arguments;

        if (args.SimulatePaymentFailure)
        {
            Console.WriteLine($"  [FAIL]       ProcessPayment — payment declined for OrderId={args.OrderId}");
            Console.WriteLine($"               ↳ Compensation will now run in reverse order...");
            return context.Faulted(new Exception("Payment declined by gateway"));
        }

        Console.WriteLine($"  [DONE]       ProcessPayment — charged ${args.Amount:F2} for OrderId={args.OrderId}");

        return context.Completed(new OrderLog
        {
            OrderId     = args.OrderId,
            Step        = "ProcessPayment",
            CompletedAt = DateTimeOffset.UtcNow
        });
    }

    public async Task<CompensationResult> Compensate(CompensateContext<OrderLog> context)
    {
        Console.WriteLine($"  [COMPENSATE] ProcessPayment  — refunding charge for OrderId={context.Log.OrderId}");
        return context.Compensated();
    }
}
