using MassTransit;

namespace SagaHost;

public class OrderState : SagaStateMachineInstance
{
    public Guid    CorrelationId           { get; set; }
    public string  CurrentState            { get; set; } = null!;
    public string  CustomerName            { get; set; } = null!;
    public decimal Amount                  { get; set; }
    public bool    SimulateShippingFailure { get; set; }
    public string? TransactionId           { get; set; }
    public string? TrackingNumber          { get; set; }
    public string? CancellationReason      { get; set; }
    public string? FailureReason           { get; set; }
}
