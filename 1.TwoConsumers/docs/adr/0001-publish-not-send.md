# ADR 0001 — Use Publish, not Send, for both demo scenarios

## Status
Accepted

## Context
Both the fan-out demo (Project 1) and the competing consumers demo (Project 3) need a publisher. The natural instinct for competing consumers is to `Send` to a named queue — the publisher "knows" where work goes. But `Send` bypasses the RabbitMQ exchange entirely, which breaks the fan-out demo and obscures the real mechanism behind competing consumers.

## Decision
Both publishers call `bus.Publish<OrderPlaced>()`. The behavior difference between fan-out and competing consumers comes entirely from whether consumers use different queue names or the same one — not from how the publisher sends.

## Consequences
- Publisher code is identical in both demos, which sharpens the teaching point: **queue name is the only variable that changes the pattern**.
- Audience sees MassTransit's exchange-based routing in the RabbitMQ UI for both demos.
- Using `Send` in either project would be wrong and must not be reintroduced.
