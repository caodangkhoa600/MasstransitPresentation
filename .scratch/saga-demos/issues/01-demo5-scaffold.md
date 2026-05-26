# Issue 01: Demo 5 — State Machine Saga scaffold

Status: completed

## Description
Create the `5.StateMachineSaga/` directory with all projects, CLAUDE.md, CONTEXT.md, SCRIPT.md,
and the ADR.

## Acceptance criteria
- [ ] `Contracts/` project with 4 event records each carrying `Guid CorrelationId`
- [ ] `SagaHost/` project with `OrderState`, `OrderStateMachine`, and `Program.cs`
- [ ] `Publisher/` project with two-scenario stepping script
- [ ] `Publisher/Publisher.slnx` referencing all three projects
- [ ] `CLAUDE.md`, `CONTEXT.md`, `SCRIPT.md`
- [ ] `docs/adr/0001-in-memory-saga-repository.md`
