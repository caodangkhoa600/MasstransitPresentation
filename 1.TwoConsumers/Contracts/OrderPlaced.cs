namespace Contracts;

public record OrderPlaced(Guid OrderId, string CustomerName, string Product);