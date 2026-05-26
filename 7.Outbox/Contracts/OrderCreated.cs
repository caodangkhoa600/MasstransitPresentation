namespace Contracts;

public record OrderCreated
{
    public Guid OrderId { get; init; }
    public string CustomerName { get; init; } = null!;
    public decimal Amount { get; init; }
}
