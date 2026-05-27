using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SagaHost;

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║     Order Saga Host  -  Starting         ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("Registered:");
Console.WriteLine("  [SAGA]      OrderStateMachine  → PostgreSQL");
Console.WriteLine("  [WORKER]    InventoryWorker");
Console.WriteLine("  [WORKER]    PaymentWorker");
Console.WriteLine("  [WORKER]    ShippingWorker");
Console.WriteLine("  [WORKER]    EmailWorker");
Console.WriteLine();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        var connStr = ctx.Configuration.GetSection("ConnectionStrings")["SagaDb"]
                      ?? "Host=localhost;Port=5432;Database=saga_demo;Username=postgres;Password=postgres";

        services.AddDbContext<OrderStateDbContext>(options =>
            options.UseNpgsql(connStr));

        services.AddMassTransit(x =>
        {
            x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    r.UsePostgres();
                    r.ExistingDbContext<OrderStateDbContext>();
                });

            x.AddConsumer<InventoryWorker>();
            x.AddConsumer<PaymentWorker>();
            x.AddConsumer<ShippingWorker>();
            x.AddConsumer<EmailWorker>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                // Retry on PostgreSQL serialization conflicts (error 40001)
                // caused by concurrent messages arriving at the same time
                cfg.UseMessageRetry(r =>
                {
                    r.Handle<Npgsql.PostgresException>(e => e.SqlState == "40001");
                    r.Intervals(50, 100, 250, 500);
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    })
    .Build();

// Auto-create table if not exists (demo only — use migrations in production)
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderStateDbContext>();
    await db.Database.EnsureCreatedAsync();
    Console.WriteLine("PostgreSQL connected. Table 'order_states' ready.");
}

Console.WriteLine("Connecting to RabbitMQ at localhost...");
Console.WriteLine("Waiting for events. Transitions appear below:");
Console.WriteLine("──────────────────────────────────────────────────────────");

await host.RunAsync();
