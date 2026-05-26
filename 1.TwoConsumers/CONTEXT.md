# CONTEXT — TwoConsumers Demo

## Glossary

**Fan-out**
When a publisher calls `bus.Publish()`, MassTransit sends the message to a RabbitMQ fanout exchange named after the message type. Every queue bound to that exchange receives a copy. The publisher has no knowledge of queue names.

**Competing Consumers**
Two or more consumer instances share the same queue name. RabbitMQ delivers each message to exactly one instance (round-robin). This is covered in Project 3, not here.

**Publish**
`bus.Publish<T>()` — the correct call for fan-out. Sends to the type-named exchange; MassTransit binds each consumer's queue to it automatically. Never use `Send` for this demo — `Send` targets a specific named queue and bypasses the exchange.

**OrderPlaced**
The single message contract for this demo: `(Guid OrderId, string CustomerName, string Product)`. Defined in the local Contracts project; not shared with other demos.

**consumer-a / consumer-b**
The hardcoded queue names for ConsumerA and ConsumerB in this demo. Each queue binds to the `OrderPlaced` exchange independently, which is what produces the fan-out behavior.
