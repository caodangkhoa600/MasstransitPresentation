using Contracts;
using MassTransit;

namespace Consumer;

public class OrderProcessedConsumer : IConsumer<OrderProcessed>
{
    public Task Consume(ConsumeContext<OrderProcessed> context)
    {
        Console.WriteLine($"  [DELIVERED]  OrderProcessed received  OrderId={context.Message.OrderId}  ProcessedAt={context.Message.ProcessedAt:HH:mm:ss.fff}");
        return Task.CompletedTask;
    }
}
