using Contracts;
using MassTransit;

namespace Activities;

public class ValidateOrderActivity : IActivity<OrderArguments, OrderLog>
{
    public async Task<ExecutionResult> Execute(ExecuteContext<OrderArguments> context)
    {
        var args = context.Arguments;

        if (string.IsNullOrWhiteSpace(args.OrderId) || args.Amount <= 0)
        {
            Console.WriteLine($"  [FAIL]       ValidateOrder  — invalid order data");
            return context.Faulted(new Exception("Order validation failed: invalid OrderId or Amount"));
        }

        Console.WriteLine($"  [DONE]       ValidateOrder  — OrderId={args.OrderId}  Amount=${args.Amount:F2}");

        return context.Completed(new OrderLog
        {
            OrderId     = args.OrderId,
            Step        = "ValidateOrder",
            CompletedAt = DateTimeOffset.UtcNow
        });
    }

    public async Task<CompensationResult> Compensate(CompensateContext<OrderLog> context)
    {
        Console.WriteLine($"  [COMPENSATE] ValidateOrder  — reversing validation for OrderId={context.Log.OrderId}");
        return context.Compensated();
    }
}
