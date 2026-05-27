namespace Contracts;

public record PaymentProcessed
{
    public Guid CorrelationId { get; init; }
    public string TransactionId { get; init; } = null!;
}
