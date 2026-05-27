using Contracts;
using MassTransit;

namespace Consumer;

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly AppDbContext _db;

    public OrderCreatedConsumer(AppDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var msg = context.Message;

        // Stage business data
        _db.Orders.Add(new OrderRecord
        {
            Id = msg.OrderId,
            CustomerName = msg.CustomerName,
            Amount = msg.Amount,
            SavedAt = DateTimeOffset.UtcNow
        });

        // Stage outgoing event — the outbox filter captures this into an OutboxMessage
        // entity on the same DbContext instead of publishing directly to the broker.
        await context.Publish(new OrderProcessed
        {
            OrderId = msg.OrderId,
            ProcessedAt = DateTimeOffset.UtcNow
        });

        Console.WriteLine($"  [DB SAVED]   OrderId={msg.OrderId}  Customer={msg.CustomerName}");
        Console.WriteLine($"  [OUTBOX]     OrderProcessed enqueued — will be delivered by relay");
    }
}
