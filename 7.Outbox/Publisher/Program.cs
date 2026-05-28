using Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║        Outbox Publisher  -  Starting                 ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("Each press of Enter publishes one OrderCreated event.");
Console.WriteLine("Watch the Consumer terminal for [DB SAVED] → [OUTBOX] → [DELIVERED]");
Console.WriteLine();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });
        });
    })
    .Build();

await host.StartAsync();

var bus = host.Services.GetRequiredService<IPublishEndpoint>();

string[] customers = ["Alice Johnson", "Bob Smith", "Carol White"];
decimal[] amounts = [249.99m, 99.50m, 499.00m];

for (var i = 0; i < customers.Length; i++)
{
    Console.WriteLine($"Press Enter to publish OrderCreated for {customers[i]}...");
    Console.ReadLine();

    var orderId = Guid.NewGuid();
    await bus.Publish(new OrderCreated
    {
        OrderId = orderId,
        CustomerName = customers[i],
        Amount = amounts[i]
    });
    Console.WriteLine($"  [SENT] OrderCreated  OrderId={orderId}  Customer={customers[i]}  Amount=${amounts[i]:F2}");
    Console.WriteLine($"         → Switch to Consumer terminal to see the outbox deliver OrderProcessed");
    Console.WriteLine();
}

Console.WriteLine("Done. Press Enter to exit.");
Console.ReadLine();

await host.StopAsync();
