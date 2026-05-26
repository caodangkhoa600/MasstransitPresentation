namespace Contracts;

public record OrderLog
{
    public string OrderId { get; init; } = null!;
    public string Step { get; init; } = null!;
    public DateTimeOffset CompletedAt { get; init; }
}
