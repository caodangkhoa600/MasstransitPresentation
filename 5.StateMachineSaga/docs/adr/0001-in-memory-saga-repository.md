# ADR 0001: In-Memory Saga Repository

## Status
Accepted

## Context
The demo must teach state machine mechanics without adding infrastructure complexity.
Three alternatives were considered for storing `OrderState` instances.

## Decision
Use `.InMemoryRepository()` from the core `MassTransit` package — a `ConcurrentDictionary`
held in process memory, requiring zero additional NuGet packages.

## Consequences
- Zero extra dependencies — the audience sees one line to register both the machine and its store
- State is lost when the SagaHost process restarts — this is intentional and demonstrated live
- The restart-loses-state scenario directly motivates Demo 7 (Outbox + persistent storage)

## Alternatives considered
- **EF Core + SQLite** — deferred to Demo 7 which adds Outbox; adding persistence here splits focus
- **Redis repository** — adds a Redis Docker container; obscures the saga concept
- **InMemoryRepository (chosen)** — zero dependencies, shows mechanics clearly, failure mode is instructive
