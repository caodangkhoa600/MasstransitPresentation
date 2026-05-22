namespace MasstransitRabbitMQ;

public interface IMassTransitPublisher
{
    Task<bool> PublishAsync<T>(T message) where T : class;
}
