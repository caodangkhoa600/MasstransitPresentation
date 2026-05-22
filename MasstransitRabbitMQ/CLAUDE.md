# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A .NET 10.0 C# console application intended as a presentation/demo project for [MassTransit](https://masstransit.io/) with RabbitMQ. The project is in its initial skeleton state.

## Build & Run Commands

```powershell
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run

# Build release
dotnet build -c Release
```

## Tech Stack

- **Language**: C# (.NET 10.0)
- **Output**: Console application (`Exe`)
- **Nullable reference types**: enabled
- **Implicit usings**: enabled
- **Intended dependencies** (not yet added): MassTransit, MassTransit.RabbitMQ

## Adding MassTransit

When implementing, add the core packages:

```powershell
dotnet add package MassTransit
dotnet add package MassTransit.RabbitMQ
```

## Architecture Intent

MassTransit on RabbitMQ follows a producer/consumer pattern:

- **Messages** — plain C# records or classes (no framework references)
- **Consumers** — implement `IConsumer<TMessage>`; registered via `AddConsumer<T>()`
- **Bus configuration** — `IBusControl` configured in `UsingRabbitMq()` block, wired up in `Program.cs`
- **Publishing** — inject `IBus` or `ISendEndpointProvider` to publish/send messages

## No Tests Yet

There is no test project. When adding tests, use xUnit with `MassTransit.Testing` (`InMemoryTestHarness` or `TestHarness`) to avoid requiring a live broker in unit tests.

## Agent skills

### Issue tracker

Issues live as local markdown files under `.scratch/`. See `docs/agents/issue-tracker.md`.

### Triage labels

Default five-role vocabulary (`needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`). See `docs/agents/triage-labels.md`.

### Domain docs

Single-context layout — `CONTEXT.md` + `docs/adr/` at the repo root. See `docs/agents/domain.md`.
