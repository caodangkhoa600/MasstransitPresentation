# Single queue for all domain events with ActionType header dispatch

The legacy system sent all domain events to a single RabbitMQ queue (`order-events`) using an `ActionType` transport header to distinguish message types, with the body varying per type. During Phase 1 migration to MassTransit we preserve this topology — one queue, one `IConsumer<DomainEvent>`, ActionType header dispatch via a switch statement — to avoid changing the queue contract while legacy publishers are still being migrated.

The idiomatic MassTransit alternative (separate `IConsumer<T>` per type, each on its own exchange) was deferred to Phase 2 because it requires coordinating a topology change across all publishers simultaneously. The `DomainEvent` wrapper and the switch statement are removed once all legacy publishers are confirmed decommissioned.

## Considered options

- **Separate consumers per type (idiomatic MassTransit)** — deferred; requires all publishers to migrate at once.
- **Single `IConsumer<DomainEvent>` with ActionType dispatch (chosen)** — preserves the one-queue constraint; allows gradual publisher migration.
