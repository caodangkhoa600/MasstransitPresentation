namespace Consumer;

public class OrderRecord
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTimeOffset SavedAt { get; set; }
}
