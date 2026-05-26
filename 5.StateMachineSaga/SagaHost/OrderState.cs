using MassTransit;

namespace SagaHost;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string? TrackingNumber { get; set; }
    public string? CancellationReason { get; set; }
}
