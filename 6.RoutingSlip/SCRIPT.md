# Demo 6: Routing Slip — Presenter Script

## Hook (~3 min)

> "We just saw Saga — great for long-running coordination where you're waiting days for a
> customer to approve something. But what about a pipeline that must complete in a tight sequence?
> Validate the order, charge the card, ship the package — in that order, no gaps."

> "And if charging the card fails, you need to undo the order validation. But if shipping fails
> after the card is charged, you need to refund the charge."

> "Routing Slip — also called Courier in MassTransit — is the pattern for this. Think of it like
> a boarding pass. The publisher stamps 'go to ValidateOrder first, then PaymentProcessing, then
> Shipping' on the message BEFORE it leaves. Each activity hands the slip to the next activity
> when it's done. The slip travels with the work."

---

## Concept explanation (~3 min)

Draw on the whiteboard:

```
Publisher
  ↓  builds RoutingSlip
  ↓  [ValidateOrder → ProcessPayment → ShipOrder]
  
ValidateOrder       → [DONE]  → hands slip to ProcessPayment
ProcessPayment      → [DONE]  → hands slip to ShipOrder
ShipOrder           → [DONE]  → routing slip completes

OR, if ProcessPayment faults:
ValidateOrder       → [DONE]  → hands slip to ProcessPayment
ProcessPayment      → [FAIL]  → slip stops forwarding
ValidateOrder       ← [COMPENSATE] ← reverse order, only completed steps
ShipOrder:          never ran, never compensated
```

> "Two key points: First, each activity has TWO methods — Execute is the happy path, Compensate
> is the undo. Second, compensation only fires for activities that COMPLETED. ShipOrder never ran,
> so it never compensates. Compensation isn't a full teardown — it's targeted."

> "The publisher doesn't know about RabbitMQ queue names at design time — it constructs the slip
> with the URI for each activity's execute queue. The activities are completely autonomous; they
> don't know about each other."

---

## Code walkthrough (~10 min)

### `Contracts/OrderArguments.cs`

> "The input to every activity in this slip. All three activities receive the same `OrderArguments`
> because they all need the order details. Notice `SimulatePaymentFailure` — this is a demo-only
> field we use to force a fault on step 2. A real system would never have this."

### `Contracts/OrderLog.cs`

> "The output from each activity. When an activity completes, it writes an `OrderLog` to the slip.
> These logs travel with the slip and are available to the compensation methods — the Compensate
> method receives the log that its own Execute wrote."

### `Activities/ValidateOrderActivity.cs`

> "Standard activity implementation. `Execute` validates the data and returns `context.Completed()`
> with an `OrderLog` entry. `Compensate` receives that same log and undoes the action.
> Notice: activities have NO knowledge of other activities. They don't know what comes before or after."

### `Activities/ProcessPaymentActivity.cs`

> "Same structure. The key difference: when `SimulatePaymentFailure` is true, it returns
> `context.Faulted()` instead of `context.Completed()`. That one call stops the routing slip and
> triggers compensation on everything that already completed — in this case, just ValidateOrder."

### `CourierHost/Program.cs`

```csharp
x.AddActivity<ValidateOrderActivity, OrderArguments, OrderLog>();
x.AddActivity<ProcessPaymentActivity, OrderArguments, OrderLog>();
x.AddActivity<ShipOrderActivity, OrderArguments, OrderLog>();
cfg.ConfigureEndpoints(context);
```

> "Four lines register three activities and auto-create six queues. Two queues per activity:
> one execute, one compensate. `ConfigureEndpoints` names them from the class name — remove
> 'Activity', convert to kebab-case, append `_execute` or `_compensate`."

Point to the queue list printed on startup. Optionally open RabbitMQ Management to show all 6 queues.

### `Publisher/Program.cs`

```csharp
var builder = new RoutingSlipBuilder(NewId.NextGuid());
builder.AddActivity("ValidateOrder",  new Uri("queue:validate-order_execute"),  args);
builder.AddActivity("ProcessPayment", new Uri("queue:process-payment_execute"), args);
builder.AddActivity("ShipOrder",      new Uri("queue:ship-order_execute"),      args);
await bus.Execute(builder.Build());
```

> "The publisher builds the entire itinerary upfront. `bus.Execute()` dispatches the first
> activity. Each activity internally forwards the slip to the next one. The publisher fires and
> forgets — it never hears back whether it succeeded."

> "If you wanted to know the outcome, you'd subscribe to `RoutingSlipCompleted` or
> `RoutingSlipFaulted` events — MassTransit publishes these automatically."

---

## Live demo steps (~10 min)

**Setup:**
1. Start CourierHost — show 6 queues printed on startup
2. Optionally open `http://localhost:15672` — Queues tab, show all 6 queues
3. Start Publisher

**Scenario A (happy path):**

4. Press Enter → routing slip dispatched
5. Switch to CourierHost — should see all three `[DONE]` lines appear in sequence
6. Point out the ORDER: ValidateOrder finishes before ProcessPayment starts (sequential, not parallel)

**Scenario B (failure + compensation):**

7. Press Enter → routing slip with `SimulatePaymentFailure = true`
8. Switch to CourierHost:
   - `[DONE] ValidateOrder`
   - `[FAIL] ProcessPayment — payment declined`
   - `[COMPENSATE] ValidateOrder — reversing validation`
9. Ask audience: "What about ShipOrder? Did it compensate?"
   - Answer: NO. ShipOrder never ran; it has nothing to compensate.
10. Ask audience: "What if we placed the fault in ShipOrder instead of ProcessPayment?"
    - Answer: BOTH ValidateOrder AND ProcessPayment would compensate, in that order.

---

## Key takeaway (30 sec)

> "Routing Slip gives you a sequential pipeline with built-in compensation — no central
> coordinator needed. Each activity is independent, testable in isolation. The slip is the
> coordination document. You define the order once in the publisher, and compensation is
> automatically scoped to whatever actually completed. Compare this to Saga: Saga waits for
> events that may arrive days apart. Routing Slip is for a tight, synchronous-style pipeline
> that must either complete or cleanly compensate."
