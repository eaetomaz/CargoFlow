using CargoFlow.Domain.Entities.Freight;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CargoFlow.Infrastructure.Persistence.Configurations.Freight;

public class FreightPricingRuleConfiguration : IEntityTypeConfiguration<FreightPricingRule>
{
    public void Configure(EntityTypeBuilder<FreightPricingRule> builder)
    {
        builder.Property(r => r.PricePerKg).HasColumnType("numeric(10,4)");
        builder.Property(r => r.PricePerKm).HasColumnType("numeric(10,4)");
        builder.Property(r => r.TollPerKm).HasColumnType("numeric(10,4)");
        builder.Property(r => r.AdValoremPercentage).HasColumnType("numeric(5,2)");
        builder.Property(r => r.GrisPercentage).HasColumnType("numeric(5,2)");
        builder.Property(r => r.MinimumFreightValue).HasColumnType("numeric(12,2)");
    }
}
