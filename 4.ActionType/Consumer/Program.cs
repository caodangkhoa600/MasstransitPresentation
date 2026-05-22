using Consumer;
using MassTransit;

var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
{
    cfg.Host("localhost", "/", h =>
    {
        h.Username("guest");
        h.Password("guest");
    });

    cfg.ReceiveEndpoint("order-events", e =>
    {
        e.Consumer<OrderEventConsumer>();
    });
});

await bus.StartAsync();

Console.WriteLine("Consumer started. Listening on queue: order-events");
Console.WriteLine("Press Enter to stop.");
Console.ReadLine();

await bus.StopAsync();
