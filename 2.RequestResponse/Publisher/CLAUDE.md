# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A .NET 10.0 C# console application that is the **Publisher** side of a Request/Response messaging demo. It is part of a three-project solution under `RequestResponse/`:

- **Publisher** — sends requests and awaits responses (this project)
- **Consumer** — handles incoming requests and returns responses (`../Consumer/`)
- **Contracts** — shared message contract types (`../Contracts/`)

The pattern demonstrated is MassTransit Request/Response over RabbitMQ.

## How to Run

**Start Consumer first, then Publisher in a second terminal.** If Publisher starts before Consumer, requests will time out.

**Terminal 1 — Consumer:**
```powershell
cd Consumer
dotnet run
```
Wait until you see `Waiting for requests.` before continuing.

**Terminal 2 — Publisher:**
```powershell
cd Publisher
dotnet run
```
Press Enter in the Publisher terminal to step through each request.

## Build Commands

```powershell
# Build the full solution from the root
dotnet build RequestResponse.slnx

# Build release
dotnet build RequestResponse.slnx -c Release
```

## Tech Stack

- **Language**: C# (.NET 10.0)
- **Messaging**: MassTransit 8.5.5 over RabbitMQ
- **Broker**: RabbitMQ at `localhost` (credentials hardcoded for demo)

## Architecture

The Request/Response pattern with MassTransit:

- **Contracts** live in `Contracts/` — plain C# records with no framework references (e.g., `GetOrderRequest`, `GetOrderResponse`)
- **Publisher** uses `IRequestClient<TRequest>` to send a request and `await` the typed response
- **Consumer** implements `IConsumer<TRequest>` and calls `context.RespondAsync<TResponse>(...)` 
- **Bus configuration** — `IBusControl` wired up in each `Program.cs` via `UsingRabbitMq()`; the Consumer host must be running before the Publisher sends

## Tests

No test project — intentionally omitted for this demo.
