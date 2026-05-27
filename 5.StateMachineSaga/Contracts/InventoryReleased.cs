namespace Contracts;

public record InventoryReleased
{
    public Guid CorrelationId { get; init; }
}
