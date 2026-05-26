# Domain Context — MassTransit ActionType Presentation

This context covers the Phase 1 bridge pattern: a MassTransit publisher and consumer sharing a single queue using ActionType header dispatch, as a stepping stone toward idiomatic per-type MassTransit routing.

## Language

**DomainEvent**:
The single message type that flows through the `order-events` queue. Carries a `Payload` (JsonElement of the concrete event) and an `ActionType` transport header that identifies the payload shape.
_Avoid_: message wrapper, base message, event envelope

**ActionType**:
A string value set on RabbitMQ transport headers (key: `"ActionType"`). Determines which concrete event type the `DomainEvent.Payload` contains.
_Avoid_: routing key, discriminator, message type header

| Value             | Concrete type           |
| ----------------- | ----------------------- |
| `order-placed`    | `OrderPlacedMessage`    |
| `order-cancelled` | `OrderCancelledMessage` |
| `order-shipped`   | `OrderShippedMessage`   |

**Payload**:
The `DomainEvent.Payload` property — a `JsonElement` holding the concrete event data. The consumer deserializes it to the correct type after reading the ActionType header. Not a string; not double-encoded.
_Avoid_: "JSON string payload", "serialized payload string"

**order-events**:
The single RabbitMQ queue all domain events are sent to. This name is a hard constraint — it must not change during migration.

**OrderPlacedMessage**:
Concrete domain event representing a customer submitting an order.
Fields: `OrderId`, `CustomerName`, `Amount`.

**OrderCancelledMessage**:
Concrete domain event representing an order being cancelled.
Fields: `OrderId`, `Reason`.

**OrderShippedMessage**:
Concrete domain event representing an order leaving the warehouse.
Fields: `OrderId`, `TrackingNumber`.

## Relationships

- A **DomainEvent** carries exactly one concrete event in its **Payload**
- The **ActionType** header on a **DomainEvent** identifies which concrete type is in **Payload**
- All **DomainEvent** messages flow through the single `order-events` queue

## Migration context

This demo is **Phase 1** of a two-phase migration away from a legacy plain RabbitMQ.Client implementation.

| Phase | Publisher | Consumer | Contract |
|-------|-----------|----------|----------|
| 1 (current) | MassTransit | MassTransit `IConsumer<DomainEvent>` | `DomainEvent` + ActionType header |
| 2 (target) | MassTransit | Separate `IConsumer<T>` per type | No `DomainEvent` wrapper |

**Phase 1 → Phase 2 trigger**: when all legacy non-MassTransit publishers writing to `order-events` are confirmed decommissioned. At that point, replace `OrderEventConsumer` with separate `IConsumer<OrderPlacedMessage>`, `IConsumer<OrderCancelledMessage>`, and `IConsumer<OrderShippedMessage>` — all bound to the same `order-events` queue via MassTransit's native routing.

## Example dialogue

> **Dev:** "Why does the publisher wrap everything in a `DomainEvent` instead of sending the concrete type directly?"
> **Domain expert:** "Because all three event types share one queue. The consumer needs the ActionType header to know which shape to deserialize — `DomainEvent` is the stable envelope while the old publishers are still live."
> **Dev:** "When do we get rid of it?"
> **Domain expert:** "When the last legacy publisher is off. Then we switch to typed consumers and MassTransit handles dispatch natively."

## Flagged ambiguities

- "Payload" was previously described as a "JSON-serialized string" — resolved: it is a `JsonElement`, not a double-encoded string.
