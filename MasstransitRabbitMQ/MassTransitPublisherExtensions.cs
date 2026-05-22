using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MasstransitRabbitMQ;

public static class MassTransitPublisherExtensions
{
    public static IServiceCollection AddMassTransitPublisher(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
        services.Configure<PublisherSettings>(configuration.GetSection("Publisher"));
        services.AddSingleton<IMassTransitPublisher, MassTransitPublisher>();

        services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);

            x.UsingRabbitMq((ctx, cfg) =>
            {
                var settings = ctx.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
                cfg.Host(settings.Host, settings.VirtualHost, h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
