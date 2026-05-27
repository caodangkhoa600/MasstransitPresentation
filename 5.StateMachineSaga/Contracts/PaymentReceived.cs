namespace Contracts;

public record PaymentReceived
{
    public Guid   CorrelationId { get; init; }
    public string TransactionId { get; init; } = null!;
}
