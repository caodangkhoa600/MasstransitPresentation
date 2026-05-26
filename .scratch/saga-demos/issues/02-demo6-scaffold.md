# Issue 02: Demo 6 — Routing Slip scaffold

Status: completed

## Description
Create the `6.RoutingSlip/` directory with all projects, docs, and SCRIPT.md.

## Acceptance criteria
- [ ] `Contracts/` project with `OrderArguments` (incl. `SimulatePaymentFailure`) and `OrderLog`
- [ ] `Activities/` project with `ValidateOrderActivity`, `ProcessPaymentActivity`, `ShipOrderActivity`
- [ ] Each activity implements both `Execute` and `Compensate`
- [ ] `ProcessPaymentActivity` faults when `SimulatePaymentFailure == true`
- [ ] `CourierHost/` registers 3 activities with `ConfigureEndpoints`
- [ ] `Publisher/` builds two routing slips (success and failure scenarios)
- [ ] `CLAUDE.md`, `CONTEXT.md`, `SCRIPT.md`
- [ ] `docs/adr/0001-compensate-on-payment-failure.md`
