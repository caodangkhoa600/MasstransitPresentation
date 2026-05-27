namespace Contracts;

public record ShipmentFailed
{
    public Guid   CorrelationId { get; init; }
    public string Reason        { get; init; } = null!;
}
