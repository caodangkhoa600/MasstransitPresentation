using MassTransit;
using Contracts;

var queueName = "consumer-b";

Console.WriteLine($"=== ConsumerB === (queue: {queueName})");
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
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[ConsumerB] ✔ OrderId: {ctx.Message.OrderId}  Customer: {ctx.Message.CustomerName}  Product: {ctx.Message.Product}");
            Console.ResetColor();
            return Task.CompletedTask;
        });
    });
});

await bus.StartAsync();
Console.ReadLine();
await bus.StopAsync();
