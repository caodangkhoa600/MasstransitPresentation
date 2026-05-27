using Contracts;
using MassTransit;

namespace SagaHost;

public class InventoryWorker : IConsumer<ReserveInventory>, IConsumer<ReleaseInventory>
{
    public async Task Consume(ConsumeContext<ReserveInventory> context)
    {
        Console.WriteLine($"  [INVENTORY] Reserving inventory...");
        await Task.Delay(800);
        await context.Publish(new InventoryReserved { CorrelationId = context.Message.CorrelationId });
        Console.WriteLine($"  [INVENTORY] Done → published InventoryReserved");
    }

    public async Task Consume(ConsumeContext<ReleaseInventory> context)
    {
        Console.WriteLine($"  [INVENTORY] RELEASING inventory...");
        await Task.Delay(800);
        await context.Publish(new InventoryReleased { CorrelationId = context.Message.CorrelationId });
        Console.WriteLine($"  [INVENTORY] Released → published InventoryReleased");
    }
}
