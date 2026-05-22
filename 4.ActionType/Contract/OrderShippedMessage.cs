namespace Contract;

public record OrderShippedMessage
{
    public string OrderId        { get; init; } = string.Empty;
    public string TrackingNumber { get; init; } = string.Empty;
}
