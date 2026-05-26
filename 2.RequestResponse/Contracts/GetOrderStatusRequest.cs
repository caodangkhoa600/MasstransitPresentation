namespace Contracts;

public record GetOrderStatusRequest
{
    public int OrderId { get; init; }
}
