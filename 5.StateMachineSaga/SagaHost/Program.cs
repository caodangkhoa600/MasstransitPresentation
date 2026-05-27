using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SagaHost;

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║     Order Saga Host  -  Starting         ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("Registered:");
Console.WriteLine("  [SAGA]      OrderStateMachine");
Console.WriteLine("  [WORKER]    InventoryWorker");
Console.WriteLine("  [WORKER]    PaymentWorker");
Console.WriteLine("  [WORKER]    ShippingWorker");
Console.WriteLine("  [WORKER]    EmailWorker");
Console.WriteLine();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                .InMemoryRepository();

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

                cfg.ConfigureEndpoints(context);
            });
        });
    })
    .Build();

Console.WriteLine("Connecting to RabbitMQ at localhost...");
Console.WriteLine("Waiting for events. Transitions appear below:");
Console.WriteLine("──────────────────────────────────────────────────────────");

await host.RunAsync();
