using Contracts;
using MassTransit;

var instanceId = Guid.NewGuid().ToString("N")[..6].ToUpper();

Console.WriteLine($"=== Consumer [{instanceId}] === (queue: order-processing)");
Console.WriteLine("Waiting for messages... Press ENTER to exit.");
Console.WriteLine();

var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
{
    cfg.Host("rabbitmq://localhost", h =>
    {
        h.Username("innova");
        h.Password("CarMD1234");
    });

    cfg.ReceiveEndpoint("order-processing", e =>
    {
        e.Handler<OrderPlaced>(ctx =>
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[{instanceId}] ✔ OrderId: {ctx.Message.OrderId}  Product: {ctx.Message.Product}");
            Console.ResetColor();
            return Task.CompletedTask;
        });
    });
});

await bus.StartAsync();
Console.ReadLine();
await bus.StopAsync();
