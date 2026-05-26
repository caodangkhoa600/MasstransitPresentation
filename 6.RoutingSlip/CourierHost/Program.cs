using Activities;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║     Routing Slip Host  -  Starting       ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.AddActivity<ValidateOrderActivity, Contracts.OrderArguments, Contracts.OrderLog>();
            x.AddActivity<ProcessPaymentActivity, Contracts.OrderArguments, Contracts.OrderLog>();
            x.AddActivity<ShipOrderActivity, Contracts.OrderArguments, Contracts.OrderLog>();

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
Console.WriteLine("Six queues created (execute + compensate per activity):");
Console.WriteLine("  validate-order_execute     validate-order_compensate");
Console.WriteLine("  process-payment_execute    process-payment_compensate");
Console.WriteLine("  ship-order_execute         ship-order_compensate");
Console.WriteLine();
Console.WriteLine("Waiting for routing slips. Press Ctrl+C to stop.");
Console.WriteLine("──────────────────────────────────────────────────");

await host.RunAsync();
