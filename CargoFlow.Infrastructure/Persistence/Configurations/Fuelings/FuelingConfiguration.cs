using CargoFlow.Domain.Entities.Fuelings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CargoFlow.Infrastructure.Persistence.Configurations.Fuelings;

public class FuelingConfiguration : IEntityTypeConfiguration<Fueling>
{
    public void Configure(EntityTypeBuilder<Fueling> builder)
    {
        builder.Property(f => f.GasStationName).HasMaxLength(200).IsRequired();
        builder.Property(f => f.LiterQuantity).HasColumnType("numeric(10,2)");
        builder.Property(f => f.TotalValue).HasColumnType("numeric(12,2)");
        builder.Property(f => f.OdometerReading).HasColumnType("numeric(10,1)");

        builder.HasOne(f => f.Vehicle)
            .WithMany()
            .HasForeignKey(f => f.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Driver)
            .WithMany()
            .HasForeignKey(f => f.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.VehicleId);
        builder.HasIndex(f => f.TripId);
    }
}
