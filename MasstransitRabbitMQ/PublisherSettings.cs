namespace MasstransitRabbitMQ;

public class PublisherSettings
{
    public int SendTimeoutSeconds { get; set; } = 3;
    public int CircuitCooldownSeconds { get; set; } = 30;
}
