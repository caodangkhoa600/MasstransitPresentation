namespace Contracts;

public record ProcessPayment
{
    public Guid CorrelationId { get; init; }
    public decimal Amount { get; init; }
}
