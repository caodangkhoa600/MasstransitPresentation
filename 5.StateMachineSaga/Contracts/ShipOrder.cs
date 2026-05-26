namespace Contracts;

public record ShipOrder
{
    public Guid CorrelationId { get; init; }
    public string TrackingNumber { get; init; } = null!;
}
