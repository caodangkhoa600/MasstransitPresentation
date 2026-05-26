# Demo 5: State Machine Saga

## What this demo shows
A `MassTransitStateMachine<OrderState>` that tracks an order across four events.
Each event carries a `CorrelationId` — MassTransit uses it to find the matching saga instance
in memory and apply the correct state transition.

## Project structure
| Project | Role |
|---------|------|
| `Contracts/` | Shared message records — zero dependencies |
| `SagaHost/` | Hosts the state machine and in-memory repository |
| `Publisher/` | Fires events step-by-step through two scenarios |

## Prerequisites
```powershell
docker run -d --hostname rabbit --name rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

## Running the demo
Open **two** terminals from `5.StateMachineSaga/`:

**Terminal 1 — SagaHost:**
```
cd SagaHost
dotnet run
```

**Terminal 2 — Publisher:**
```
cd Publisher
dotnet run
```

Follow the Publisher prompts and switch to the SagaHost terminal after each step to see state transitions.

## Key observation
Kill the SagaHost mid-sequence and restart it. The saga state is gone — `.InMemoryRepository()`
stores instances in process memory only. This is the motivation for Demo 7 (Outbox + persistent storage).

## Agent skills
Issue tracker: `.scratch/<slug>/`. Triage: needs-triage, ready-for-agent, ready-for-human.
Domain docs: `CONTEXT.md`.
