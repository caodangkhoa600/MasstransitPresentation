using MasstransitRabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SamplePublisher;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddMassTransitPublisher(context.Configuration, x =>
        {
            x.AddConsumer<SampleOrderConsumer>();
        });
    })
    .Build();

await host.StartAsync();

Console.WriteLine("[SamplePublisher] Started. Publishing SampleOrder messages...\n");

var publisher = host.Services.GetRequiredService<IMassTransitPublisher>();

for (int i = 1; i <= 3; i++)
{
    var order = new SampleOrder(Guid.NewGuid(), $"Sample Customer {i}", i * 50m);
    var success = await publisher.PublishAsync(order);
    Console.WriteLine($"[Publisher] {(success ? "Sent" : "FAILED — circuit open")}: {order.CustomerName}, ${order.Amount}");
    await Task.Delay(800);
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();

await host.StopAsync();
