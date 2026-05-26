# ADR 0002: Outbox Over Manual Retry

## Status
Accepted

## Context
Several simpler approaches exist to reduce (but not eliminate) message loss between a DB save
and a message publish.

## Decision
Demonstrate the Outbox pattern as the correct solution, not retry or try/catch.

## Why retry is insufficient
- **Try/catch + retry**: the catch block only fires if the exception is caught in the same
  process run. A `System.OutOfMemoryException`, OS kill, or power loss terminates the process
  before any retry fires. Retry does not close the crash window.
- **Idempotent consumer (at-least-once delivery)**: handles the re-delivery side but not the
  lost-publish side. Idempotency means "I can safely process the same message twice"; it does
  not help if the message was never published.
- **Two-phase commit across DB + broker**: theoretically closes the window, but RabbitMQ does
  not support XA transactions. Even MSMQ/MSDTC, which do, cause tight infrastructure coupling
  and cascading failure risks.

## Consequences
Outbox is the canonical production-grade solution. The demo shows the correct answer without
exploring the dead ends, but the SCRIPT.md mentions retry explicitly so the presenter can
address it when audience members suggest it.
