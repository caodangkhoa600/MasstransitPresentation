using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SagaHost;

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║     Order Saga Host  -  Starting         ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                .InMemoryRepository();

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
Console.WriteLine("State machine registered. Waiting for events.");
Console.WriteLine("State transitions will appear below:\n");
Console.WriteLine("──────────────────────────────────────────────────");

await host.RunAsync();
