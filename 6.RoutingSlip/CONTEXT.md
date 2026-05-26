# Routing Slip (Courier) — Domain Context

## Glossary

**Routing Slip**
An immutable document built by the publisher listing the activities to execute in order, plus the
arguments each activity needs. Once dispatched, activities stamp their completion log onto the slip.
Avoid: "workflow definition", "pipeline config".

**Activity**
A unit of work in a routing slip. Implements `IActivity<TArguments, TLog>` — two generic
parameters: the input arguments and the log entry written on completion. Each activity generates
exactly one log entry when it succeeds. Avoid: "step", "handler", "consumer".

**Compensate**
The undo operation for a completed activity. MassTransit calls compensate on already-completed
activities in reverse order when a downstream activity faults. Compensation is a deliberate
corrective action — not a rollback. It assumes the original action already happened.
Avoid: "rollback", "undo transaction".

**RoutingSlipBuilder**
The fluent builder the publisher uses to construct a routing slip. Caller specifies the ordered
list of activity execute-queue URIs and the arguments to pass each one.
Avoid: "routing slip factory", "slip creator".

**Execute Queue**
The RabbitMQ queue an activity listens on. Named `{activity-name}_execute` by MassTransit's
`ConfigureEndpoints` (e.g., `validate-order_execute`). Avoid: "activity queue", "work queue".

**Compensate Queue**
The RabbitMQ queue an activity listens on for compensation messages. Named
`{activity-name}_compensate`. Only receives messages if a downstream activity faulted.
Avoid: "rollback queue".

**SimulatePaymentFailure**
A boolean in `OrderArguments`. When `true`, `ProcessPaymentActivity.Execute` returns
`context.Faulted(...)`. Demo-only toggle — not a real domain concept.

## Compensation scope

Compensation only runs on activities that **already completed**. Activities that never ran
are never compensated.

**Scenario B breakdown:**
1. ValidateOrder executes → completes (log written)
2. ProcessPayment executes → **faults** (no log written)
3. ShipOrder never executes
4. Compensation runs backward: only ValidateOrder is compensated
