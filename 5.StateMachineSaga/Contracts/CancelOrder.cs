namespace Contracts;

public record CancelOrder
{
    public Guid CorrelationId { get; init; }
    public string Reason { get; init; } = null!;
}
