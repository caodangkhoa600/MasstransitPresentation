using Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });
        });
    })
    .Build();

await host.StartAsync();

var bus = host.Services.GetRequiredService<IPublishEndpoint>();

Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║        Order Saga Publisher  -  Starting             ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("  A — Happy path      : SubmitOrder → (wait for payment) → PaymentReceived → Shipped ✓");
Console.WriteLine("  B — Cancellation    : SubmitOrder → CancelOrder mid-flight");
Console.WriteLine("  C — Shipping fails  : SubmitOrder → ShipmentFailed → compensation → Failed ✗");
Console.WriteLine();
Console.WriteLine("──────────────────────────────────────────────────────────────────");
Console.WriteLine();

// ── Scenario A: Happy path with human payment ─────────────────────────────────
var orderA = Guid.NewGuid();
Console.WriteLine($"SCENARIO A  CorrelationId = {orderA}");
Console.WriteLine("Press Enter to send SubmitOrder...");
Console.ReadLine();

await bus.Publish(new SubmitOrder { CorrelationId = orderA, CustomerName = "Alice Johnson", Amount = 249.99m });
Console.WriteLine($"  [SENT] SubmitOrder");
Console.WriteLine($"         → SagaHost: reserves inventory, sends payment link email");
Console.WriteLine($"         → Saga now sitting in 'WaitingForPayment' state...");

Console.WriteLine("\nPress Enter to simulate user clicking the link and paying...");
Console.ReadLine();

var txId = $"TXN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
await bus.Publish(new PaymentReceived { CorrelationId = orderA, TransactionId = txId });
Console.WriteLine($"  [SENT] PaymentReceived (TransactionId: {txId})");
Console.WriteLine($"         → Saga wakes up → creates shipment → Shipped ✓");

Console.WriteLine("\nPress Enter when done to start Scenario B...");
Console.ReadLine();

// ── Scenario B: Cancel mid-flight ─────────────────────────────────────────────
var orderB = Guid.NewGuid();
Console.WriteLine($"\nSCENARIO B  CorrelationId = {orderB}");
Console.WriteLine("Press Enter to send SubmitOrder...");
Console.ReadLine();

await bus.Publish(new SubmitOrder { CorrelationId = orderB, CustomerName = "Bob Smith", Amount = 99.50m });
Console.WriteLine($"  [SENT] SubmitOrder — press Enter to cancel while waiting for payment...");

Console.ReadLine();
await bus.Publish(new CancelOrder { CorrelationId = orderB, Reason = "Customer changed mind" });
Console.WriteLine($"  [SENT] CancelOrder");

Console.WriteLine("\nPress Enter when done to start Scenario C...");
Console.ReadLine();

// ── Scenario C: Shipping fails → compensation ─────────────────────────────────
var orderC = Guid.NewGuid();
Console.WriteLine($"\nSCENARIO C  CorrelationId = {orderC}");
Console.WriteLine("This order will fail at the Shipping step.");
Console.WriteLine("Watch the Saga compensate: refund payment → release inventory.");
Console.WriteLine();
Console.WriteLine("Press Enter to send SubmitOrder (SimulateShippingFailure = true)...");
Console.ReadLine();

await bus.Publish(new SubmitOrder
{
    CorrelationId = orderC,
    CustomerName = "Charlie Brown",
    Amount = 149.00m,
    SimulateShippingFailure = true
});
Console.WriteLine($"  [SENT] SubmitOrder (SimulateShippingFailure=true)");
Console.WriteLine($"         → Saga reserves inventory, sends payment link...");

Console.WriteLine("\nPress Enter to simulate Charlie paying...");
Console.ReadLine();

var txIdC = $"TXN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
await bus.Publish(new PaymentReceived { CorrelationId = orderC, TransactionId = txIdC });
Console.WriteLine($"  [SENT] PaymentReceived (TransactionId: {txIdC})");
Console.WriteLine($"         → Saga tries to ship → ShipmentFailed → compensation runs automatically");

Console.WriteLine("\nDone. Press Enter to exit.");
Console.ReadLine();

await host.StopAsync();
