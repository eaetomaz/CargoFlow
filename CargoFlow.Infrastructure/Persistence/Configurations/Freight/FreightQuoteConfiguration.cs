using CargoFlow.Domain.Entities.Freight;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CargoFlow.Infrastructure.Persistence.Configurations.Freight;

public class FreightQuoteConfiguration : IEntityTypeConfiguration<FreightQuote>
{
    public void Configure(EntityTypeBuilder<FreightQuote> builder)
    {
        builder.HasOne(q => q.CustomerCompany)
            .WithMany()
            .HasForeignKey(q => q.CustomerCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(q => q.CargoWeightKg).HasColumnType("numeric(10,2)");
        builder.Property(q => q.CargoValue).HasColumnType("numeric(12,2)");
        builder.Property(q => q.EstimatedDistanceKm).HasColumnType("numeric(10,1)");
        builder.Property(q => q.FreightWeightValue).HasColumnType("numeric(12,2)");
        builder.Property(q => q.TollValue).HasColumnType("numeric(12,2)");
        builder.Property(q => q.AdValoremValue).HasColumnType("numeric(12,2)");
        builder.Property(q => q.GrisValue).HasColumnType("numeric(12,2)");
        builder.Property(q => q.OtherCostsValue).HasColumnType("numeric(12,2)");
        builder.Property(q => q.TotalValue).HasColumnType("numeric(12,2)");

        builder.HasIndex(q => q.Status);
    }
}
