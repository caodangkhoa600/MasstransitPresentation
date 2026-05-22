using MassTransit;

namespace SamplePublisher;

public class SampleOrderConsumer : IConsumer<SampleOrder>
{
    public Task Consume(ConsumeContext<SampleOrder> context)
    {
        var order = context.Message;
        Console.WriteLine($"[Consumer] Received: {order.OrderId} — {order.CustomerName}, ${order.Amount}");
        return Task.CompletedTask;
    }
}
