namespace Contracts;

public record CreateShipment
{
    public Guid CorrelationId { get; init; }
    public string CustomerName { get; init; } = null!;
    public bool SimulateFailure { get; init; }
}
