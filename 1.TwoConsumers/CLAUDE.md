# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**TwoConsumerForOneMessage** is a MassTransit + RabbitMQ demonstration project showing what happens when one publisher sends a message and two consumers are running. It contrasts two fundamentally different behaviors depending on queue configuration.

## Projects

```
Contracts/   — shared message record (OrderPlaced); referenced by all three apps
Publisher/   — publishes OrderPlaced via bus.Publish()
ConsumerA/   — receives messages; queue name controlled by CLI arg
ConsumerB/   — receives messages; queue name controlled by CLI arg
```

`Publisher/Publisher.slnx` only includes the Publisher project. There is no top-level solution spanning all four projects.

## Prerequisites

RabbitMQ must be running locally on the default port (5672) with guest/guest credentials.

```powershell
docker run -d --hostname rabbit --name rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

## Commands

```powershell
# Restore & build all projects
dotnet build Publisher
dotnet build ConsumerA
dotnet build ConsumerB

# Run (open each in a separate terminal)
dotnet run --project Publisher
dotnet run --project ConsumerA
dotnet run --project ConsumerB
```

## Demo Script

### Scenario 1 — Fan-out: both consumers receive the message

Each consumer has its own queue. MassTransit creates a binding from the exchange to both queues.

```
Terminal 1:  dotnet run --project ConsumerA          # queue: consumer-a
Terminal 2:  dotnet run --project ConsumerB          # queue: consumer-b
Terminal 3:  dotnet run --project Publisher          # press ENTER to send
```

**Result:** Both ConsumerA (cyan) and ConsumerB (yellow) print the same OrderId.

### Scenario 2 — Competing consumers: only one receives the message

Both consumers share the same queue. RabbitMQ round-robins deliveries between them.

```
Terminal 1:  dotnet run --project ConsumerA -- shared-queue
Terminal 2:  dotnet run --project ConsumerB -- shared-queue
Terminal 3:  dotnet run --project Publisher          # press ENTER repeatedly
```

**Result:** Each message is printed by either ConsumerA or ConsumerB — never both.

## Architecture

- `Contracts/OrderPlaced.cs` — the single message record (`Guid OrderId, string CustomerName, string Product`)
- Publisher calls `bus.Publish<OrderPlaced>()` — MassTransit sends to the fanout exchange named after the type
- Consumers call `cfg.ReceiveEndpoint(queueName, ...)` — the queue name determines fan-out vs competing behavior
- No dependency injection or hosted-service wrapper; all three use the bare `Bus.Factory.CreateUsingRabbitMq` API for simplicity
