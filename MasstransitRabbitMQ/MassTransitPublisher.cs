using MassTransit;
using Microsoft.Extensions.Options;

namespace MasstransitRabbitMQ;

public sealed class MassTransitPublisher : IMassTransitPublisher
{
    private readonly TimeSpan _sendTimeout;
    private readonly TimeSpan _circuitCooldown;

    // Thread-safe circuit breaker state using Interlocked operations.
    // _errorTimeTicks: ticks of the last failure (0 = no error). Updated on every failure so cooldown
    //                  always counts from the most recent attempt, not the first.
    // _notificationSent: flipped 0→1 by the thread that wins the first-failure race; prevents duplicate emails.
    private static long _errorTimeTicks = 0;
    private static int _notificationSent = 0;

    private readonly IBus _bus;

    public MassTransitPublisher(IBus bus, IOptions<PublisherSettings> settings)
    {
        _bus = bus;
        _sendTimeout = TimeSpan.FromSeconds(settings.Value.SendTimeoutSeconds);
        _circuitCooldown = TimeSpan.FromSeconds(settings.Value.CircuitCooldownSeconds);
    }

    public async Task<bool> PublishAsync<T>(T message) where T : class
    {
        long errorTicks = Interlocked.Read(ref _errorTimeTicks);
        if (errorTicks != 0 && DateTime.UtcNow.Ticks - errorTicks < _circuitCooldown.Ticks)
            return false;

        try
        {
            using var cts = new CancellationTokenSource(_sendTimeout);
            await _bus.Publish(message, cts.Token);

            Interlocked.Exchange(ref _errorTimeTicks, 0);
            Interlocked.Exchange(ref _notificationSent, 0);
            return true;
        }
        catch (Exception ex)
        {
            Interlocked.Exchange(ref _errorTimeTicks, DateTime.UtcNow.Ticks);

            if (Interlocked.CompareExchange(ref _notificationSent, 1, 0) == 0)
            {
                try
                {
                    // TODO: send error notification email
                    _ = ex;
                }
                catch (Exception emailEx)
                {
                    // FileLogger.LogError(emailEx, "Failed to send RabbitMQ error notification email");
                    _ = emailEx;
                }
            }

            return false;
        }
    }
}
