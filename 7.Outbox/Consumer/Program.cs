using Consumer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║     Outbox Consumer  -  Starting         ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        var connStr = ctx.Configuration.GetConnectionString("OutboxDb");

        services.AddDbContext<AppDbContext>(o =>
            o.UseNpgsql(connStr));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderCreatedConsumer, OrderCreatedConsumerDefinition>();
            x.AddConsumer<OrderProcessedConsumer>();

            // Wire all context.Publish() calls inside consumers through the outbox.
            // Both the business data (OrderRecord) and the outgoing message (OrderProcessed)
            // are committed in a single database transaction.
            x.AddEntityFrameworkOutbox<AppDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    })
    .Build();

// Create the database and all tables (including the three outbox tables)
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    Console.WriteLine("Database ready: PostgreSQL (outbox_demo)");
    Console.WriteLine("Tables: Orders, InboxState, OutboxMessage, OutboxState");
    Console.WriteLine();
}

Console.WriteLine("Connecting to RabbitMQ at localhost...");
Console.WriteLine("Waiting for OrderCreated events.");
Console.WriteLine("Flow: [DB SAVED] → [OUTBOX enqueued] → (relay) → [DELIVERED]");
Console.WriteLine("──────────────────────────────────────────────────");

await host.RunAsync();
