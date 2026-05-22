namespace Contract;

public record OrderPlacedMessage
{
    public string  OrderId      { get; init; } = string.Empty;
    public string  CustomerName { get; init; } = string.Empty;
    public decimal Amount       { get; init; }
}
