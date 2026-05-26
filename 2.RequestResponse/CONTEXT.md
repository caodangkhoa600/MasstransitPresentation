# Bounded Context: Request/Response Demo

A MassTransit demonstration of the Request/Response messaging pattern over RabbitMQ.

---

## Glossary

### NotFound
A domain-level status value (`Status = "NotFound"`) returned in a `GetOrderStatusResponse` when the requested `OrderId` does not exist in the Order Store. The Consumer always calls `RespondAsync` — including for unknown orders. The Publisher inspects the status field and branches on it. This is distinct from a messaging-level timeout, which would only occur if the Consumer were unreachable.

### Order Store
A `static readonly Dictionary<int, (string Status, string CustomerName, decimal TotalAmount)>` inside `GetOrderStatusConsumer`. Intentionally hardcoded for demo predictability — no real database dependency required. Seeded with three orders (IDs 1001, 1002, 1003).

### Contracts
The shared class library (`Contracts/`) that holds plain C# `record` types representing the message interface between Publisher and Consumer. Records carry no framework references — they are pure data shapes. The canonical term is **Contracts** (not "Contacts", which was a typo in the original project).

### Publisher
The console application that initiates communication. It uses MassTransit's `IRequestClient<TRequest>` to send a typed request and synchronously awaits a correlated response.

### Consumer
The console application that handles incoming requests. It implements `IConsumer<TRequest>` and calls `context.RespondAsync()` to return a typed response. MassTransit handles queue provisioning automatically via `ConfigureEndpoints`.

### Request/Response Pattern
A synchronous-style messaging pattern where the Publisher sends a request and blocks until a correlated response arrives (or a timeout fires). MassTransit correlates requests and responses automatically using a correlation ID embedded in the message envelope.
