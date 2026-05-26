# ADR 0001: File-Based SQLite for Demo Database

## Status
Accepted

## Context
Demo 7 requires a relational database that supports transactions (the outbox guarantee depends
on committing business data and outbox message in ONE transaction).

## Decision
Use `Microsoft.EntityFrameworkCore.Sqlite` with `Data Source=outbox-demo.db` (a file on disk).

## Consequences
- Real SQLite file created at `outbox-demo.db` — the presenter can open it with DB Browser for SQLite
- Real ACID transactions — the outbox atomicity guarantee actually holds
- File persists between runs, demonstrating that outbox rows survive restarts
- Delete `outbox-demo.db` manually to reset between demo runs

## Alternatives considered
- **EF Core InMemory provider** — rejected: does not support transactions; the outbox writes
  would not be atomic. The core guarantee of the pattern would silently not apply.
- **SQLite in-memory (`Data Source=:memory:`)** — rejected: each DbContext connection creates
  a new empty database unless connections are explicitly shared. Sharing a single connection
  adds code complexity without teaching benefit.
- **SQL Server / Postgres** — adds a Docker container prerequisite beyond RabbitMQ; the demo
  teaches MassTransit configuration, not database setup
- **File-based SQLite (chosen)** — real transactions, zero extra infra, inspectable file
