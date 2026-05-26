# ADR 0001: Fault Placed at Step 2 (ProcessPayment)

## Status
Accepted

## Context
The demo needs one failure scenario to show compensation. The fault can be placed at any of the
three activity steps.

## Decision
Place the fault at step 2 (`ProcessPaymentActivity`) with step 1 already completed and step 3
never reached.

## Consequences
- Compensation fires only for step 1 (ValidateOrder) — clearly shows the asymmetry
- Step 3 (ShipOrder) never appears in output, making it obvious to the audience that
  "compensation only runs on completed steps"
- The audience can reason about "what if the fault was at step 3?" as a Q&A exercise

## Alternatives considered
- **Fault at step 3 (ShipOrder)** — compensation fires for both steps 1 and 2, which is more
  dramatic but obscures the selective nature of compensation (everything compensates, so
  it looks like a full teardown)
- **Fault at step 1 (ValidateOrder)** — nothing to compensate; less instructive
- **Fault at step 2 (chosen)** — cleanest teaching scenario for showing partial compensation
