namespace Contracts;

public record SubmitOrder
{
    public Guid CorrelationId { get; init; }
    public string CustomerName { get; init; } = null!;
    public decimal Amount { get; init; }
}
