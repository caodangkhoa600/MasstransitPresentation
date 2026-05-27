namespace Contracts;

public record RefundPayment
{
    public Guid    CorrelationId  { get; init; }
    public decimal Amount         { get; init; }
    public string  TransactionId  { get; init; } = null!;
}
