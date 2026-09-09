using CargoFlow.Domain.Entities.Fleet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CargoFlow.Infrastructure.Persistence.Configurations.Fleet;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.Property(v => v.PlateNumber).HasMaxLength(10).IsRequired();
        builder.HasIndex(v => v.PlateNumber).IsUnique();

        builder.Property(v => v.Renavam).HasMaxLength(20).IsRequired();
        builder.HasIndex(v => v.Renavam).IsUnique();

        builder.Property(v => v.CapacityKg).HasColumnType("numeric(10,2)");
        builder.Property(v => v.TareWeightKg).HasColumnType("numeric(10,2)");
        builder.Property(v => v.Odometer).HasColumnType("numeric(10,1)");
    }
}
