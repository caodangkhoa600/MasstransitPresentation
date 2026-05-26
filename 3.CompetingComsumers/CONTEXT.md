# CONTEXT — CompetingConsumers Demo

## Glossary

**Competing Consumers**
Multiple instances of the same consumer application share one queue. RabbitMQ delivers each message to exactly one instance (round-robin). No instance receives a message that another already handled.

**order-processing**
The shared queue name all consumer instances bind to. Using the same name is what causes competing-consumer behavior — RabbitMQ sees one queue with multiple subscribers and distributes load across them.

**Instance ID**
A short random hex string (e.g. `A3F9C2`) generated at startup and printed on every log line. Lets the audience see which running instance received each message without tracking terminal positions.

**Publish**
`bus.Publish<T>()` — the publisher sends to the RabbitMQ exchange named after the message type. It has no knowledge of `order-processing` or how many consumers exist. The competing behavior comes entirely from consumers sharing one queue name, not from how the publisher sends.

**OrderPlaced**
The single message contract for this demo: `(Guid OrderId, string CustomerName, string Product)`. Defined in the local Contracts project; not shared with other demos.
