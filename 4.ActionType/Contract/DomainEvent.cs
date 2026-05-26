using System.Text.Json;

namespace Contract;

public record DomainEvent
{
    public JsonElement Payload { get; init; }
}
