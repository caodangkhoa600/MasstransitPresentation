# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A .NET 10.0 C# console application that is the **Publisher** side of a Request/Response messaging demo. It is part of a three-project solution under `RequestResponse/`:

- **Publisher** — sends requests and awaits responses (this project)
- **Consumer** — handles incoming requests and returns responses (`../Consumer/`)
- **Contacts** — shared message contract types (`../Contacts/`)

All three projects are currently skeleton placeholders (`Hello, World!`). The intended pattern is MassTransit Request/Response over RabbitMQ (see the sibling `MasstransitRabbitMQ` presentation project for reference).

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

To build or run the full solution, run the same commands from `../` (the `RequestResponse/` directory once a `.slnx` exists there).

## Tech Stack

- **Language**: C# (.NET 10.0)
- **Output**: Console application (`Exe`)
- **Nullable reference types**: enabled
- **Implicit usings**: enabled
- **Intended dependencies** (not yet added): MassTransit, MassTransit.RabbitMQ

## Adding MassTransit

When implementing, add the core packages to both Publisher and Consumer:

```powershell
dotnet add package MassTransit
dotnet add package MassTransit.RabbitMQ
```

## Architecture Intent

The Request/Response pattern with MassTransit:

- **Contracts** live in `Contacts/` — plain C# records with no framework references (e.g., `GetOrderRequest`, `GetOrderResponse`)
- **Publisher** uses `IRequestClient<TRequest>` to send a request and `await` the typed response
- **Consumer** implements `IConsumer<TRequest>` and calls `context.RespondAsync<TResponse>(...)` 
- **Bus configuration** — `IBusControl` wired up in each `Program.cs` via `UsingRabbitMq()`; the Consumer host must be running before the Publisher sends

## No Tests Yet

No test project exists. When adding tests, use xUnit with `MassTransit.Testing` (`TestHarness`) to avoid requiring a live broker.
