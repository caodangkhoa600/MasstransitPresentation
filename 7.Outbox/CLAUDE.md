# Demo 7: Transactional Outbox

## What this demo shows
The dual-write problem: saving to a database and publishing a message are two separate operations.
Without an outbox, a crash between them silently loses the message. MassTransit's EF Core outbox
writes both in one atomic database transaction, then a relay delivers the message.

## Project structure
| Project | Role |
|---------|------|
| `Contracts/` | `OrderCreated` (inbound) and `OrderProcessed` (outbound via outbox) |
| `Consumer/` | Hosts `OrderCreatedConsumer` (saves + publishes via outbox) and `OrderProcessedConsumer` (proves delivery) |
| `Publisher/` | Publishes `OrderCreated` events |

## Prerequisites
```powershell
docker run -d --hostname rabbit --name rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

## Running the demo
Open **two** terminals from `7.Outbox/`:

**Terminal 1 — Consumer:**
```
cd Consumer
dotnet run
```
Creates `outbox-demo.db` (SQLite) with 4 tables: `Orders`, `InboxState`, `OutboxMessage`, `OutboxState`.

**Terminal 2 — Publisher:**
```
cd Publisher
dotnet run
```
Press Enter for each order. Watch Consumer terminal for the delivery sequence.

## Expected output (Consumer terminal)
```
  [DB SAVED]   OrderId=...  Customer=Alice Johnson
  [OUTBOX]     OrderProcessed enqueued — will be delivered by relay
  [DELIVERED]  OrderProcessed received  OrderId=...
```

## Key observation
The `[OUTBOX]` and `[DELIVERED]` lines may appear nearly simultaneously — the relay is fast.
What matters is that they are **decoupled**: DB write and message delivery are two separate phases,
but if the process crashes after the DB write, the relay delivers on restart because the
`OutboxMessage` row is already committed.

To inspect the database: install the `sqlite3` CLI or DB Browser for SQLite, then open `outbox-demo.db`.

## Agent skills
Issue tracker: `.scratch/<slug>/`. Triage: needs-triage, ready-for-agent, ready-for-human.
Domain docs: `CONTEXT.md`.
