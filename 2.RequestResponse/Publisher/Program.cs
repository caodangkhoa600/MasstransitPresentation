using Contacts;
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

foreach (var orderId in orderIds)
{
    Console.WriteLine($"[Publisher] --> Sending request for OrderId: {orderId}");

    try
    {
        var response = await client.GetResponse<GetOrderStatusResponse>(
            new GetOrderStatusRequest { OrderId = orderId });

        var msg = response.Message;
        Console.WriteLine($"[Publisher] <-- Response received:");
        Console.WriteLine($"            Status   : {msg.Status}");
        Console.WriteLine($"            Customer : {msg.CustomerName}");
        Console.WriteLine($"            Total    : ${msg.TotalAmount:F2}");
    }
    catch (RequestTimeoutException)
    {
        Console.WriteLine($"[Publisher] <-- TIMEOUT: no response for OrderId {orderId}");
    }
    Console.ReadLine();
}

await host.StopAsync();
Console.WriteLine("Done.");
