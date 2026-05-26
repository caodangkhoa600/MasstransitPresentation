# Transactional Outbox — Domain Context

## Glossary

**Dual Write Problem**
The scenario where a service must update a database AND publish a message, and these two operations
are not atomic. If the process crashes between the DB save and the `bus.Publish()` call, the DB is
updated but no message is ever sent — a silent data loss.
Avoid: "two-phase commit problem", "atomicity problem".

**Outbox**
A table in the service's own database where outgoing messages are written as part of the same
database transaction as the business data. A relay then reads from this table and publishes to
the broker. The message is only sent AFTER the DB row is committed — and if the relay fails, it
retries from the persisted row.
Avoid: "message queue", "staging area".

**Transactional Outbox**
The full pattern: the outbox table lives in the same relational database as the business data,
and the write to both is a single atomic transaction. The relay is the only process that touches
the broker.
Avoid conflating with "inbox" (the read side for idempotency, which MassTransit also provides).

**AddEntityFrameworkOutbox**
The MassTransit registration that adds outbox support to a `DbContext`. Configures three outbox
tables (`OutboxMessage`, `OutboxState`, `InboxState`) and wires the consumer pipeline.
Package: `MassTransit.EntityFrameworkCore`.

**UseBusOutbox**
A fluent option on `AddEntityFrameworkOutbox` that routes all `context.Publish()` and
`context.Send()` calls made inside a consumer through the outbox instead of directly to the broker.
Without this, publish calls inside a consumer still bypass the outbox.

**InboxState**
A MassTransit table that records which messages have already been consumed. Provides consumer-side
idempotency — if a message is re-delivered (RabbitMQ at-least-once), the inbox state prevents
double-processing. Distinct from the outbox (which is the publish side).

**Relay**
The `OutboxDeliveryService` background service, started automatically by `AddEntityFrameworkOutbox`.
It polls `OutboxMessage` for undelivered rows and publishes them to RabbitMQ. It marks rows
delivered only after the broker confirms receipt.
Avoid: "outbox worker", "delivery service".

## What the code does NOT demonstrate

The full crash-and-recover scenario is not demonstrated live (crashing the process at the exact right
moment is fragile in a demo setting). The guarantee holds because:

1. `OrderRecord` + `OutboxMessage` are committed in ONE transaction
2. If the process crashes after commit, the `OutboxMessage` row persists in `outbox-demo.db`
3. On restart, the relay finds the row and delivers it
4. The InboxState row prevents the consumer from processing it a second time if the original
   RabbitMQ message is re-delivered
