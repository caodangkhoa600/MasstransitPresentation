using MassTransit;
using Contracts;

// Scenario 1 — fan-out    : dotnet run (uses default queue "consumer-a")
// Scenario 2 — competing  : dotnet run -- shared-queue
var queueName = args.Length > 0 ? args[0] : "consumer-a";

Console.WriteLine($"=== ConsumerA === (queue: {queueName})");
Console.WriteLine("Waiting for messages... Press ENTER to exit.");
Console.WriteLine();

var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
{
    cfg.Host("rabbitmq://localhost", h =>
    {
        h.Username("innova");
        h.Password("CarMD1234");
    });

    cfg.ReceiveEndpoint(queueName, e =>
    {
        e.Handler<OrderPlaced>(ctx =>
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[ConsumerA] ✔ OrderId: {ctx.Message.OrderId}  Customer: {ctx.Message.CustomerName}  Product: {ctx.Message.Product}");
            Console.ResetColor();
            return Task.CompletedTask;
        });
    });
});

await bus.StartAsync();
Console.ReadLine();
await bus.StopAsync();
