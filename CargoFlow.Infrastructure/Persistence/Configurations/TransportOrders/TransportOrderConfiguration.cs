using CargoFlow.Domain.Entities.TransportOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CargoFlow.Infrastructure.Persistence.Configurations.TransportOrders;

public class TransportOrderConfiguration : IEntityTypeConfiguration<TransportOrder>
{
    public void Configure(EntityTypeBuilder<TransportOrder> builder)
    {
        builder.HasOne(o => o.CustomerCompany)
            .WithMany()
            .HasForeignKey(o => o.CustomerCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(o => o.CargoWeightKg).HasColumnType("numeric(10,2)");
        builder.Property(o => o.CargoValue).HasColumnType("numeric(12,2)");
        builder.Property(o => o.FreightValue).HasColumnType("numeric(12,2)");

        builder.HasIndex(o => o.Status);
    }
}
