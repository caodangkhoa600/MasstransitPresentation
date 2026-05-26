# Demo 6: Routing Slip (Courier)

## What this demo shows
A MassTransit Routing Slip that chains three activities in order. Each activity implements
`IActivity<TArguments, TLog>` with both an `Execute` and a `Compensate` method. When
`ProcessPaymentActivity` faults (via a demo flag), MassTransit runs compensation in reverse
on every activity that already completed.

## Project structure
| Project | Role |
|---------|------|
| `Contracts/` | `OrderArguments` (input) and `OrderLog` (per-activity completion record) |
| `Activities/` | Three activities: ValidateOrder, ProcessPayment, ShipOrder |
| `CourierHost/` | Hosts all three activities and auto-creates 6 RabbitMQ queues |
| `Publisher/` | Builds routing slips and dispatches them |

## Prerequisites
```powershell
docker run -d --hostname rabbit --name rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

## Running the demo
Open **two** terminals from `6.RoutingSlip/`:

**Terminal 1 — CourierHost:**
```
cd CourierHost
dotnet run
```

**Terminal 2 — Publisher:**
```
cd Publisher
dotnet run
```

## Expected output

**Scenario A (happy path):**
```
  [DONE]       ValidateOrder  — OrderId=ORD-001  Amount=$249.99
  [DONE]       ProcessPayment — charged $249.99 for OrderId=ORD-001
  [DONE]       ShipOrder      — OrderId=ORD-001  Tracking=TRACK-...
```

**Scenario B (failure + compensation):**
```
  [DONE]       ValidateOrder  — OrderId=ORD-002
  [FAIL]       ProcessPayment — payment declined for OrderId=ORD-002
               ↳ Compensation will now run in reverse order...
  [COMPENSATE] ValidateOrder  — reversing validation for OrderId=ORD-002
```
Note: `ShipOrder` never appears — it was never executed.

## Agent skills
Issue tracker: `.scratch/<slug>/`. Triage: needs-triage, ready-for-agent, ready-for-human.
Domain docs: `CONTEXT.md`.
