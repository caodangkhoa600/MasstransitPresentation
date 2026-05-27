using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Consumer;

public class OrderCreatedConsumerDefinition : ConsumerDefinition<OrderCreatedConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<OrderCreatedConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<AppDbContext>(context);
    }
}
