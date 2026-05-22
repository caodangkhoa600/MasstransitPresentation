namespace Contract;

public record DomainEvent
{
    public string Payload { get; init; } = string.Empty;
}
