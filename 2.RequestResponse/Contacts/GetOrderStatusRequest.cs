namespace Contacts;

public record GetOrderStatusRequest
{
    public int OrderId { get; init; }
}
