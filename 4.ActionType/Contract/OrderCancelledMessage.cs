namespace Contract;

public record OrderCancelledMessage
{
    public string OrderId { get; init; } = string.Empty;
    public string Reason  { get; init; } = string.Empty;
}
