namespace Contracts;

public record OrderArguments
{
    public string OrderId { get; init; } = null!;
    public string CustomerName { get; init; } = null!;
    public decimal Amount { get; init; }

    /// <summary>
    /// Demo-only toggle. When true, ProcessPaymentActivity returns Faulted to trigger compensation.
    /// </summary>
    public bool SimulatePaymentFailure { get; init; }
}
