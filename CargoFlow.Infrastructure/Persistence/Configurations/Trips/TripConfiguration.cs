using CargoFlow.Domain.Entities.Trips;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CargoFlow.Infrastructure.Persistence.Configurations.Trips;

public class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.HasOne(t => t.Vehicle)
            .WithMany()
            .HasForeignKey(t => t.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Driver)
            .WithMany()
            .HasForeignKey(t => t.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Expenses)
            .WithOne(e => e.Trip)
            .HasForeignKey(e => e.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Events)
            .WithOne(e => e.Trip)
            .HasForeignKey(e => e.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.PlannedDistanceKm).HasColumnType("numeric(10,1)");
        builder.Property(t => t.ActualDistanceKm).HasColumnType("numeric(10,1)");
        builder.Property(t => t.FreightRevenue).HasColumnType("numeric(12,2)");

        builder.HasIndex(t => t.Status);

        // MediatR.Contracts.INotification (usado pra domain events) não é
        // uma propriedade mapeável -- ignora explicitamente pra evitar que
        // o EF Core tente descobrir uma coluna pra isso.
        builder.Ignore(t => t.DomainEvents);
    }
}

public class TripExpenseConfiguration : IEntityTypeConfiguration<TripExpense>
{
    public void Configure(EntityTypeBuilder<TripExpense> builder)
    {
        builder.Property(e => e.Value).HasColumnType("numeric(12,2)");
    }
}
