using Contracts;
using MassTransit;

namespace SagaHost;

public class PaymentWorker : IConsumer<ProcessPayment>, IConsumer<RefundPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        Console.WriteLine($"  [PAYMENT]   Processing ${context.Message.Amount:F2}...");
        await Task.Delay(800);
        var txId = $"TXN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        await context.Publish(new PaymentProcessed
        {
            CorrelationId = context.Message.CorrelationId,
            TransactionId = txId
        });
        Console.WriteLine($"  [PAYMENT]   Done → published PaymentProcessed (TxId: {txId})");
    }

    public async Task Consume(ConsumeContext<RefundPayment> context)
    {
        Console.WriteLine($"  [PAYMENT]   REFUNDING ${context.Message.Amount:F2} (TxId: {context.Message.TransactionId})...");
        await Task.Delay(800);
        await context.Publish(new PaymentRefunded { CorrelationId = context.Message.CorrelationId });
        Console.WriteLine($"  [PAYMENT]   Refund done → published PaymentRefunded");
    }
}
