namespace Contracts;

public record PaymentRefunded
{
    public Guid CorrelationId { get; init; }
}
