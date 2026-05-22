namespace Contacts;

public record GetOrderStatusResponse
{
    public int OrderId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
}
