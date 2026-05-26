namespace Contracts;

public record AcceptOrder
{
    public Guid CorrelationId { get; init; }
}
