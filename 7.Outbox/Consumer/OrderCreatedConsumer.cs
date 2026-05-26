using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

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

        // Step 1: save business data to the database
        _db.Orders.Add(new OrderRecord
        {
            Id           = msg.OrderId,
            CustomerName = msg.CustomerName,
            Amount       = msg.Amount,
            SavedAt      = DateTimeOffset.UtcNow
        });

        // Step 2: publish a downstream event
        // With UseBusOutbox(), this Publish is written to the OutboxMessage table
        // in the SAME database transaction as the OrderRecord above.
        await context.Publish(new OrderProcessed
        {
            OrderId     = msg.OrderId,
            ProcessedAt = DateTimeOffset.UtcNow
        });

        Console.WriteLine($"  [DB SAVED]   OrderId={msg.OrderId}  Customer={msg.CustomerName}");
        Console.WriteLine($"  [OUTBOX]     OrderProcessed enqueued — will be delivered by relay");
        // Note: SaveChangesAsync is called by MassTransit's outbox middleware AFTER this method
        // returns, committing both OrderRecord and OutboxMessage in one atomic transaction.
    }
}
