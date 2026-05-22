using Contracts;
using MassTransit;

Console.WriteLine("=== Publisher (RabbitMQ) ===");
Console.WriteLine("Press ENTER to send a message, 'q' + ENTER to quit.");
Console.WriteLine();

var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
{
    cfg.Host("rabbitmq://localhost", h =>
    {
        h.Username("innova");
        h.Password("CarMD1234");
    });
});

await bus.StartAsync();

while (true)
{
    var input = Console.ReadLine();
    if (input?.Equals("q", StringComparison.OrdinalIgnoreCase) == true)
        break;

    var order = new OrderPlaced(Guid.NewGuid(), "Placed", $"Widget-{DateTime.Now:HH:mm:ss}");
    // Publish
    // await bus.Publish(order);
    //Console.ReadLine();

    // Send
    var endpoint = await bus.GetSendEndpoint(new Uri("queue:consumer-a"));
    await endpoint.Send(order);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[Publisher] Sent  → OrderId: {order.OrderId}  Product: {order.Product}");
    Console.ResetColor();
}

await bus.StopAsync();
