# Domain Context — MassTransit ActionType Presentation

## Glossary

### DomainEvent
The envelope record that flows through the RabbitMQ queue. Contains a `Payload` (JSON-serialized string of the concrete event). The `ActionType` transport header — not the message body — determines how the consumer routes the message.

**Do not use:** "message wrapper", "base message", "event envelope" — use "DomainEvent".

### ActionType
A string value set on the RabbitMQ transport headers (key: `"ActionType"`). Read by the consumer to determine which concrete event type the `DomainEvent.Payload` contains and how to handle it.

| Value             | Concrete type           |
| ----------------- | ----------------------- |
| `order-placed`    | `OrderPlacedMessage`    |
| `order-cancelled` | `OrderCancelledMessage` |
| `order-shipped`   | `OrderShippedMessage`   |

**Do not use:** "routing key", "discriminator", "message type header" — use "ActionType header".

### Payload
The `DomainEvent.Payload` property — a JSON-serialized string of a concrete event. The consumer deserializes it to the correct type after reading the ActionType header.

### order-events
The name of the single RabbitMQ queue all domain events are sent to. The publisher sends directly to this queue by name; the consumer listens on this queue.

### OrderPlacedMessage
Concrete domain event. Represents a customer submitting an order.
Fields: `OrderId`, `CustomerName`, `Amount`.

### OrderCancelledMessage
Concrete domain event. Represents an order being cancelled.
Fields: `OrderId`, `Reason`.

### OrderShippedMessage
Concrete domain event. Represents an order leaving the warehouse.
Fields: `OrderId`, `TrackingNumber`.
