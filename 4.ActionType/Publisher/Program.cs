using System.Text.Json;
using Contract;
using MassTransit;

var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
{
    cfg.Host("localhost", "/", h =>
    {
        h.Username("guest");
        h.Password("guest");
    });
});

await bus.StartAsync();

var endpoint = await bus.GetSendEndpoint(new Uri("queue:order-events"));

Console.WriteLine("Publishing 3 domain events to queue: order-events");
Console.WriteLine("──────────────────────────────────────────────────");

var orderPlaced = new OrderPlacedMessage
{
    OrderId      = "ORD-001",
    CustomerName = "Alice Johnson",
    Amount       = 249.99m
};
await endpoint.Send(
    new DomainEvent { Payload = JsonSerializer.SerializeToElement(orderPlaced) },
    ctx => ctx.Headers.Set("ActionType", ActionTypes.OrderPlaced));
Console.WriteLine($"[SENT] ActionType={ActionTypes.OrderPlaced,-18} OrderId={orderPlaced.OrderId}");
Console.ReadLine();

var orderCancelled = new OrderCancelledMessage
{
    OrderId = "ORD-002",
    Reason  = "Customer request"
};
await endpoint.Send(
    new DomainEvent { Payload = JsonSerializer.SerializeToElement(orderCancelled) },
    ctx => ctx.Headers.Set("ActionType", ActionTypes.OrderCancelled));
Console.WriteLine($"[SENT] ActionType={ActionTypes.OrderCancelled,-18} OrderId={orderCancelled.OrderId}");
Console.ReadLine();

var orderShipped = new OrderShippedMessage
{
    OrderId        = "ORD-003",
    TrackingNumber = "TRACK-XYZ-9876"
};
await endpoint.Send(
    new DomainEvent { Payload = JsonSerializer.SerializeToElement(orderShipped) },
    ctx => ctx.Headers.Set("ActionType", ActionTypes.OrderShipped));
Console.WriteLine($"[SENT] ActionType={ActionTypes.OrderShipped,-18} OrderId={orderShipped.OrderId}");
Console.ReadLine();

Console.WriteLine("──────────────────────────────────────────────────");
Console.WriteLine("Done. Press Enter to exit.");
Console.ReadLine();

await bus.StopAsync();
