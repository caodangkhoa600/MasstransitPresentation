using Contracts;
using MassTransit;

namespace SagaHost;

public class ShippingWorker : IConsumer<CreateShipment>
{
    public async Task Consume(ConsumeContext<CreateShipment> context)
    {
        Console.WriteLine($"  [SHIPPING]  Creating shipment for {context.Message.CustomerName}...");
        await Task.Delay(800);

        if (context.Message.SimulateFailure)
        {
            Console.WriteLine($"  [SHIPPING]  FAILED → published ShipmentFailed");
            await context.Publish(new ShipmentFailed
            {
                CorrelationId = context.Message.CorrelationId,
                Reason        = "Warehouse out of capacity"
            });
            return;
        }

        var tracking = $"TRACK-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        await context.Publish(new ShipmentCreated
        {
            CorrelationId  = context.Message.CorrelationId,
            TrackingNumber = tracking
        });
        Console.WriteLine($"  [SHIPPING]  Done → published ShipmentCreated (Tracking: {tracking})");
    }
}
