using Consumer;
using MassTransit;
using Microsoft.Extensions.Hosting;

Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║   Order Status Consumer - Starting   ║");
Console.WriteLine("╚══════════════════════════════════════╝");
Console.WriteLine();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            // Register the consumer that handles GetOrderStatusRequest
            x.AddConsumer<GetOrderStatusConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("innova");
                    h.Password("CarMD1234");
                });

                // Auto-create a queue named after the consumer (get-order-status)
                cfg.ConfigureEndpoints(context);
            });
        });
    })
    .Build();

Console.WriteLine("Connecting to RabbitMQ at localhost...");
Console.WriteLine("Waiting for requests. Press Ctrl+C to stop.\n");

await host.RunAsync();
