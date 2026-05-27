namespace Contracts;

public record RequestPayment
{
    public Guid    CorrelationId { get; init; }
    public string  CustomerName  { get; init; } = null!;
    public decimal Amount        { get; init; }
    public string  PaymentLink   { get; init; } = null!;
}
