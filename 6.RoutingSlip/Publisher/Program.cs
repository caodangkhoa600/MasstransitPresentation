using Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║       Routing Slip Publisher  -  Starting            ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("Two scenarios:");
Console.WriteLine("  A — Happy path:   ValidateOrder → ProcessPayment → ShipOrder (all succeed)");
Console.WriteLine("  B — Failure path: ValidateOrder → ProcessPayment FAILS → compensation runs");
Console.WriteLine();

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

var bus = host.Services.GetRequiredService<IBus>();

// Activity execute queue URIs — auto-created by CourierHost's ConfigureEndpoints
var validateUri  = new Uri("queue:validate-order_execute");
var paymentUri   = new Uri("queue:process-payment_execute");
var shipUri      = new Uri("queue:ship-order_execute");

// ── Scenario A: Happy path ────────────────────────────────────────────────────
Console.WriteLine("SCENARIO A  (SimulatePaymentFailure = false)");
Console.WriteLine("Press Enter to execute the routing slip...");
Console.ReadLine();

var builderA = new RoutingSlipBuilder(NewId.NextGuid());
var argsA = new OrderArguments
{
    OrderId              = "ORD-001",
    CustomerName         = "Alice Johnson",
    Amount               = 249.99m,
    SimulatePaymentFailure = false
};
builderA.AddActivity("ValidateOrder",  validateUri, argsA);
builderA.AddActivity("ProcessPayment", paymentUri,  argsA);
builderA.AddActivity("ShipOrder",      shipUri,     argsA);

await bus.Execute(builderA.Build());
Console.WriteLine("[SENT] Routing slip dispatched — watch CourierHost terminal");

Console.WriteLine("\n── Scenario A complete ──────────────────────────────────────────────────────\n");

// ── Scenario B: Payment failure + compensation ────────────────────────────────
Console.WriteLine("SCENARIO B  (SimulatePaymentFailure = true)");
Console.WriteLine("Press Enter to execute the routing slip...");
Console.ReadLine();

var builderB = new RoutingSlipBuilder(NewId.NextGuid());
var argsB = new OrderArguments
{
    OrderId                = "ORD-002",
    CustomerName           = "Bob Smith",
    Amount                 = 99.50m,
    SimulatePaymentFailure = true
};
builderB.AddActivity("ValidateOrder",  validateUri, argsB);
builderB.AddActivity("ProcessPayment", paymentUri,  argsB);
builderB.AddActivity("ShipOrder",      shipUri,     argsB);

await bus.Execute(builderB.Build());
Console.WriteLine("[SENT] Routing slip dispatched — watch CourierHost terminal");
Console.WriteLine("       Expected: ValidateOrder completes, ProcessPayment FAILS,");
Console.WriteLine("                 ValidateOrder compensates. ShipOrder never runs.");

Console.WriteLine("\n── Scenario B complete ──────────────────────────────────────────────────────\n");
Console.WriteLine("Done. Press Enter to exit.");
Console.ReadLine();

await host.StopAsync();
