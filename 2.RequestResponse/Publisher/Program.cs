using Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║   Order Status Publisher - Starting  ║");
Console.WriteLine("╚══════════════════════════════════════╝");
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
                    h.Username("innova");
                    h.Password("CarMD1234");
                });
            });
        });
    })
    .Build();

await host.StartAsync();

// IRequestClient sends the message and awaits a correlated reply
var client = host.Services.GetRequiredService<IRequestClient<GetOrderStatusRequest>>();

int[] orderIds = [1001, 1002, 1003, 9999];

for (var i = 0; i < orderIds.Length; i++)
{
    var orderId = orderIds[i];
    Console.WriteLine($"[Publisher] --> Sending request for OrderId: {orderId}");

    var response = await client.GetResponse<GetOrderStatusResponse>(
        new GetOrderStatusRequest { OrderId = orderId });

    var msg = response.Message;
    if (msg.Status == "NotFound")
    {
        Console.WriteLine($"[Publisher] <-- Order {orderId} not found.");
    }
    else
    {
        Console.WriteLine($"[Publisher] <-- Response received:");
        Console.WriteLine($"            Status   : {msg.Status}");
        Console.WriteLine($"            Customer : {msg.CustomerName}");
        Console.WriteLine($"            Total    : ${msg.TotalAmount:F2}");
    }

    if (i < orderIds.Length - 1)
    {
        Console.WriteLine("[Publisher] Press Enter to send next request...");
        Console.ReadLine();
    }
}

await host.StopAsync();
Console.WriteLine("Done.");
