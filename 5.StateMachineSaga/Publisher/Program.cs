using Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║        Order Saga Publisher  -  Starting             ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("Two scenarios:");
Console.WriteLine("  A — SubmitOrder → AcceptOrder → ShipOrder (happy path)");
Console.WriteLine("  B — SubmitOrder → CancelOrder");
Console.WriteLine();
Console.WriteLine("After each step, switch to the SagaHost terminal to see the state change.");
Console.WriteLine("──────────────────────────────────────────────────────────────────");
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

var bus = host.Services.GetRequiredService<IPublishEndpoint>();

// ── Scenario A: Happy path ────────────────────────────────────────────────────
var orderA = Guid.NewGuid();
Console.WriteLine($"SCENARIO A  CorrelationId = {orderA}");
Console.WriteLine();

Console.WriteLine("Press Enter to send [1/3] SubmitOrder...");
Console.ReadLine();
await bus.Publish(new SubmitOrder { CorrelationId = orderA, CustomerName = "Alice Johnson", Amount = 249.99m });
Console.WriteLine($"  [SENT] SubmitOrder   CustomerName=Alice Johnson  Amount=$249.99");

Console.WriteLine("\nPress Enter to send [2/3] AcceptOrder...");
Console.ReadLine();
await bus.Publish(new AcceptOrder { CorrelationId = orderA });
Console.WriteLine($"  [SENT] AcceptOrder");

Console.WriteLine("\nPress Enter to send [3/3] ShipOrder...");
Console.ReadLine();
await bus.Publish(new ShipOrder { CorrelationId = orderA, TrackingNumber = "TRACK-ABC-1234" });
Console.WriteLine($"  [SENT] ShipOrder     TrackingNumber=TRACK-ABC-1234");

Console.WriteLine("\n── Scenario A complete ──────────────────────────────────────────────────────\n");

// ── Scenario B: Cancellation ──────────────────────────────────────────────────
var orderB = Guid.NewGuid();
Console.WriteLine($"SCENARIO B  CorrelationId = {orderB}");
Console.WriteLine();

Console.WriteLine("Press Enter to send [1/2] SubmitOrder...");
Console.ReadLine();
await bus.Publish(new SubmitOrder { CorrelationId = orderB, CustomerName = "Bob Smith", Amount = 99.50m });
Console.WriteLine($"  [SENT] SubmitOrder   CustomerName=Bob Smith  Amount=$99.50");

Console.WriteLine("\nPress Enter to send [2/2] CancelOrder...");
Console.ReadLine();
await bus.Publish(new CancelOrder { CorrelationId = orderB, Reason = "Customer changed mind" });
Console.WriteLine($"  [SENT] CancelOrder   Reason=Customer changed mind");

Console.WriteLine("\n── Scenario B complete ──────────────────────────────────────────────────────\n");
Console.WriteLine("Done. Press Enter to exit.");
Console.ReadLine();

await host.StopAsync();
