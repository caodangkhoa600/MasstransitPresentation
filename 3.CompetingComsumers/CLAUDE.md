# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## Project Overview

**CompetingConsumers** demonstrates what happens when multiple instances of the same consumer share one RabbitMQ queue. Each message is delivered to exactly one instance — RabbitMQ round-robins across all connected consumers.

## Projects

```
Contracts/   — shared message record (OrderPlaced); referenced by Publisher and Consumer
Publisher/   — publishes OrderPlaced via bus.Publish()
Consumer/    — run this twice in separate terminals to see competing behavior
```

## Prerequisites

RabbitMQ must be running locally on the default port (5672) with innova/CarMD1234 credentials.

```powershell
docker run -d --hostname rabbit --name rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

## Commands

```powershell
dotnet build Publisher
dotnet build Consumer

# Open three terminals:
dotnet run --project Consumer   # Terminal 1 — prints its instance ID on startup
dotnet run --project Consumer   # Terminal 2 — different instance ID
dotnet run --project Publisher  # Terminal 3 — press ENTER to send messages
```

## Demo Script

Run two Consumer instances and one Publisher. Each time you press ENTER, the Publisher sends one message. Watch which Consumer terminal lights up — only one receives each message. Press ENTER repeatedly to see RabbitMQ distribute load between the two instances.

**Key point for the audience:** The Publisher calls `bus.Publish()` with no knowledge of `order-processing` or how many consumers exist. The competing behavior is entirely a function of consumers sharing the same queue name.

## Architecture

- `Contracts/Program.cs` — the single message record (`Guid OrderId, string CustomerName, string Product`)
- Publisher calls `bus.Publish<OrderPlaced>()` — sends to the fanout exchange, not to any queue directly
- Consumer binds to `order-processing` queue — sharing this name across instances produces competing behavior
- Instance ID is a 6-char random hex string generated at startup, prefixed on every log line
