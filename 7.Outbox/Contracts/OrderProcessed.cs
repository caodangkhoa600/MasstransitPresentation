namespace Contracts;

public record OrderProcessed
{
    public Guid OrderId { get; init; }
    public DateTimeOffset ProcessedAt { get; init; }
}
