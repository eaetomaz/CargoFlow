using CargoFlow.Domain.Entities.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CargoFlow.Infrastructure.Persistence.Configurations.Maintenance;

public class MaintenanceOrderConfiguration : IEntityTypeConfiguration<MaintenanceOrder>
{
    public void Configure(EntityTypeBuilder<MaintenanceOrder> builder)
    {
        builder.Property(o => o.Description).HasMaxLength(2000).IsRequired();
        builder.Property(o => o.ServiceProvider).HasMaxLength(200);
        builder.Property(o => o.OdometerAtService).HasColumnType("numeric(10,1)");
        builder.Property(o => o.Cost).HasColumnType("numeric(12,2)");

        builder.HasOne(o => o.Vehicle)
            .WithMany()
            .HasForeignKey(o => o.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => o.Status);
    }
}
