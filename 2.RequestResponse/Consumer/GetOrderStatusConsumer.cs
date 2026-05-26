using Contracts;
using MassTransit;

namespace Consumer;

public class GetOrderStatusConsumer : IConsumer<GetOrderStatusRequest>
{
    private static readonly Dictionary<int, (string Status, string CustomerName, decimal TotalAmount)> Orders = new()
    {
        { 1001, ("Processing", "Alice Johnson",  125.50m) },
        { 1002, ("Shipped",    "Bob Smith",       89.99m) },
        { 1003, ("Delivered",  "Carol White",    244.00m) },
    };

    public async Task Consume(ConsumeContext<GetOrderStatusRequest> context)
    {
        var orderId = context.Message.OrderId;
        Console.WriteLine($"[Consumer] Received request for OrderId: {orderId}");

        Console.WriteLine($"[Consumer] Looking up order {orderId}...");
        await Task.Delay(200);

        if (Orders.TryGetValue(orderId, out var order))
        {
            await context.RespondAsync(new GetOrderStatusResponse
            {
                OrderId      = orderId,
                Status       = order.Status,
                CustomerName = order.CustomerName,
                TotalAmount  = order.TotalAmount,
            });
        }
        else
        {
            await context.RespondAsync(new GetOrderStatusResponse
            {
                OrderId = orderId,
                Status  = "NotFound",
            });
        }

        Console.WriteLine($"[Consumer] Response sent for OrderId: {orderId}");
    }
}
