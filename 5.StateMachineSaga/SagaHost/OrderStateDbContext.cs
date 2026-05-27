using Microsoft.EntityFrameworkCore;

namespace SagaHost;

public class OrderStateDbContext : DbContext
{
    public OrderStateDbContext(DbContextOptions<OrderStateDbContext> options) : base(options) { }

    public DbSet<OrderState> OrderStates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderState>(entity =>
        {
            entity.HasKey(x => x.CorrelationId);
            entity.ToTable("order_states");

            entity.Property(x => x.CurrentState)      .HasMaxLength(64);
            entity.Property(x => x.CustomerName)       .HasMaxLength(256);
            entity.Property(x => x.TransactionId)      .HasMaxLength(128);
            entity.Property(x => x.TrackingNumber)     .HasMaxLength(128);
            entity.Property(x => x.CancellationReason) .HasMaxLength(512);
            entity.Property(x => x.FailureReason)      .HasMaxLength(512);
        });
    }
}
