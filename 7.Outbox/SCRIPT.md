# Demo 7: Transactional Outbox — Presenter Script

## Hook (~3 min)

> "Final pattern. I want you to think about something that happens in every microservice:
> you receive a message, you save something to the database, and then you publish an event.
> Two operations. Not one."

Ask: "What happens if the process crashes between the save and the publish?"

> "The database row exists. The message was never sent. The downstream service never knows.
> The order sits in your database forever with no invoice, no shipment, no notification.
> You won't see this in testing because tests don't randomly crash. You'll see it in production,
> once, at 3am, and spend four hours figuring out why the data is inconsistent."

> "The Outbox pattern is the answer. It makes the DB write and the message publish atomic — using
> your own database as the intermediary. MassTransit wires this in two registration lines."

> "Some teams try to fix this with retry logic. But retry requires knowing the publish failed.
> If the process CRASHED, the retry never fires. The Outbox is the only reliable solution
> that doesn't require a distributed transaction protocol."

---

## Concept explanation (~3 min)

Draw on the whiteboard:

**Without outbox:**
```
Consumer.Consume()
  ├── SaveToDatabase()   ← commit happens here
  └── bus.Publish()      ← CRASH HERE → message lost forever
```

**With outbox:**
```
Consumer.Consume()
  ├── db.Orders.Add(...)     \
  └── context.Publish(...)    } ← both written to DB in ONE transaction
                               /
Background relay reads OutboxMessage table → publishes to RabbitMQ → marks row delivered
```

> "The key insight: the outbox table lives in YOUR database. Writing to both the business table
> and the outbox table is a single `SaveChangesAsync()` call. If the process crashes after that
> commit, the relay picks up the row on restart. The broker never needs to be reachable at the
> moment you commit your business data."

---

## Code walkthrough (~10 min)

### `Consumer/AppDbContext.cs`

> "Standard EF Core DbContext with one addition — three MassTransit outbox tables:"
```csharp
modelBuilder.AddInboxStateEntity();
modelBuilder.AddOutboxMessageEntity();
modelBuilder.AddOutboxStateEntity();
```
> "These are real database tables in your schema, not magic. You can query them, inspect them,
> back them up. `InboxState` tracks which messages you've already consumed — idempotency.
> `OutboxMessage` holds the pending outgoing messages. `OutboxState` tracks the delivery status."

### `Consumer/OrderCreatedConsumer.cs`

Show the entire `Consume` method:
> "Looks like a completely normal consumer. We add an `OrderRecord` to the DbContext and call
> `context.Publish()`. NOTHING unusual. The outbox is transparent — you write the same code
> you'd write without it. The framework handles the atomicity."

Highlight the comment:
> "Note the comment: `SaveChangesAsync` is called by MassTransit's middleware AFTER this method
> returns, committing BOTH the `OrderRecord` and the `OutboxMessage` in one transaction.
> The developer doesn't call `SaveChangesAsync` manually."

### `Consumer/Program.cs`

Show the two key lines:
```csharp
x.AddEntityFrameworkOutbox<AppDbContext>(o =>
{
    o.UseSqlite();
    o.UseBusOutbox();  // ← routes context.Publish() through outbox
});
```
> "Two lines. `UseBusOutbox()` is the critical one — it intercepts all `context.Publish()` calls
> inside consumers and redirects them through the outbox table. Without it, publish calls still
> go directly to RabbitMQ and you have the dual-write gap."

Show the database setup:
```csharp
await db.Database.EnsureCreatedAsync();
```
> "This creates `outbox-demo.db` with all four tables. In production you'd use EF Migrations."

### `Consumer/OrderProcessedConsumer.cs`

> "This consumer proves end-to-end delivery. It receives `OrderProcessed` — which was published
> by `OrderCreatedConsumer` via the outbox. The fact that this consumer receives it means the
> relay successfully delivered it from the outbox table to RabbitMQ."

---

## Live demo steps (~10 min)

**Setup:**
1. Start Consumer — show `outbox-demo.db` created, 4 tables listed
2. Start Publisher

**Normal flow (3 orders):**

3. Press Enter → Alice Johnson's order
4. Switch to Consumer terminal:
   - `[DB SAVED]` appears first
   - `[OUTBOX]` appears (relay enqueued)
   - `[DELIVERED]` appears (relay delivered, downstream received)
5. Pause between the second and third order — observe whether `[DELIVERED]` is nearly instant
   (it usually is, since the relay runs continuously)

**Database inspection (optional):**

6. In a third terminal: `sqlite3 outbox-demo.db "SELECT * FROM OutboxMessage;"` or open with
   DB Browser for SQLite
7. Notice that delivered rows may be cleaned up by the relay, or show the `DeliveredAt` timestamp
   being populated

**Crash guarantee explanation:**

8. After the third order, explain:
   > "If I had killed this process right after `[DB SAVED]` but before `[DELIVERED]`, the
   > `OrderProcessed` row would still be in `OutboxMessage`. On restart, the relay would find
   > it and deliver it. The consumer's `InboxState` would also prevent the original `OrderCreated`
   > from being processed twice if RabbitMQ re-delivers it."

9. Ask audience: "Where does this fit with Demo 5 (Saga)?"
   > "A Saga that processes events — like `SubmitOrder` or `ShipOrder` — needs those events to
   > reliably arrive. The Outbox ensures that the service PUBLISHING those events doesn't lose
   > them in a crash. Outbox and Saga are complementary."

---

## Key takeaway (30 sec)

> "Every service that writes to a database and publishes a message should be using an outbox.
> Without it, you have a silent data-loss window. MassTransit's EF Core outbox closes that
> window in two registration lines. The consumer code is identical — the framework handles
> the rest. In production, swap SQLite for Postgres or SQL Server."

---

## Narrative arc close

> "Let me connect the three patterns we've just seen."
> 
> "**State Machine Saga** answers: how do you coordinate long-running business processes across
> multiple services? Events drive state transitions; the saga persists where each order is.
> 
> **Routing Slip** answers: how do you execute a sequential pipeline with automatic compensation
> on failure? The routing slip carries the itinerary; each activity is isolated and compensatable.
> 
> **Outbox** answers: how do you guarantee that the messages driving both of those patterns
> are never silently lost? You write to the broker and the DB atomically, using your own
> database as the durability layer.
> 
> Saga coordinates. Routing Slip sequences. Outbox ensures the messages never get lost."
