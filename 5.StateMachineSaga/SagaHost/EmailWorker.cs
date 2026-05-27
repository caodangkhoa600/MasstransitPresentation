using Contracts;
using MassTransit;

namespace SagaHost;

public class EmailWorker : IConsumer<RequestPayment>
{
    public Task Consume(ConsumeContext<RequestPayment> context)
    {
        Console.WriteLine($"  [EMAIL]     Sending payment link to {context.Message.CustomerName}...");
        Console.WriteLine($"  [EMAIL]     Link: {context.Message.PaymentLink}");
        Console.WriteLine($"  [EMAIL]     Amount: ${context.Message.Amount:F2}");
        Console.WriteLine($"  [EMAIL]     → Waiting for user to pay...");
        return Task.CompletedTask;
    }
}
