namespace Contracts;

public record OrderPlaced(Guid OrderId, string CustomerName, string Product);


public record OrderSubmitted(Guid OrderId, string CustomerName, string Product);