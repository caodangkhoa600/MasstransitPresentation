# Order Saga — Domain Context

## Glossary

**Saga**
A long-running, stateful coordination of multiple messages. A saga instance is created when the
first correlated event arrives and persists across subsequent events until it reaches a terminal state.
Avoid: "workflow", "orchestrator", "process manager".

**SagaStateMachineInstance**
The persisted state object for a single saga instance. One instance per `CorrelationId`. Contains
`CurrentState` (a string managed by MassTransit) plus business data accumulated across events.
Avoid: "saga entity", "state document".

**CorrelationId**
The `Guid` that links all events belonging to the same saga instance. All four contracts carry
`CorrelationId`. MassTransit uses `CorrelateById` to find or create the matching `OrderState`.
Avoid: "order ID", "transaction ID".

**MassTransitStateMachine**
The base class for defining state machine behavior — declares States, Events, and transition logic
(`Initially`, `During`, `DuringAny`). Avoid: "state machine class", "saga class".

**InMemoryRepository**
The saga persistence store for this demo — a `ConcurrentDictionary` held in process memory. Data
is lost when the host restarts. Included in core `MassTransit`; no extra NuGet needed.
Avoid: "in-memory database", "memory store".

**State**
A named value (`Submitted`, `Accepted`, `Shipped`, `Cancelled`) stored as a string in
`OrderState.CurrentState`. MassTransit manages transitions; never set `CurrentState` directly.

**Event (saga context)**
A message type that causes a state transition. Declared via `Event<TMessage>` and triggers a
`When(...)` clause. Distinct from the general messaging concept of "event".

## State transitions

```
              ┌────────────┐
              │  (Initial) │
              └─────┬──────┘
       SubmitOrder  │
              ┌─────▼──────┐
              │  Submitted │
              └──┬──────┬──┘
    AcceptOrder  │      │ CancelOrder
          ┌──────▼──┐   │
          │ Accepted│   │
          └──┬───┬──┘   │
  ShipOrder  │   │Cncl  │
        ┌────▼─┐ ▼      ▼
        │Shpd  │   ┌──────────┐
        └──────┘   │Cancelled │
                   └──────────┘
```
