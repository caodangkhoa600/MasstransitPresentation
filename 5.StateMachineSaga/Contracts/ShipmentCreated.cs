namespace Contracts;

public record ShipmentCreated
{
    public Guid CorrelationId { get; init; }
    public string TrackingNumber { get; init; } = null!;
}
