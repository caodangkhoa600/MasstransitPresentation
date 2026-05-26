# Issue 03: Demo 7 — Transactional Outbox scaffold

Status: completed

## Description
Create the `7.Outbox/` directory with EF Core + MassTransit outbox wiring.

## Acceptance criteria
- [ ] `Contracts/` project with `OrderCreated` and `OrderProcessed`
- [ ] `Consumer/AppDbContext.cs` with `AddInboxStateEntity`, `AddOutboxMessageEntity`, `AddOutboxStateEntity`
- [ ] `Consumer/OrderRecord.cs` EF entity
- [ ] `Consumer/OrderCreatedConsumer.cs` saves to DB + publishes via outbox
- [ ] `Consumer/OrderProcessedConsumer.cs` proves downstream delivery
- [ ] `Consumer/Program.cs` with `AddEntityFrameworkOutbox` + `UseBusOutbox()` + `EnsureCreatedAsync`
- [ ] `Publisher/` publishes 3 orders with Enter-to-continue stepping
- [ ] `CLAUDE.md`, `CONTEXT.md`, `SCRIPT.md`
- [ ] `docs/adr/0001-file-sqlite-for-demo.md`
- [ ] `docs/adr/0002-outbox-over-manual-retry.md`
