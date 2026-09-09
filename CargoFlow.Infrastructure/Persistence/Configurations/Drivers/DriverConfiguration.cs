using CargoFlow.Domain.Entities.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CargoFlow.Infrastructure.Persistence.Configurations.Drivers;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.Property(d => d.Name).HasMaxLength(200).IsRequired();

        builder.Property(d => d.Cpf).HasMaxLength(14).IsRequired();
        builder.HasIndex(d => d.Cpf).IsUnique();

        builder.Property(d => d.CnhNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(d => d.CnhNumber).IsUnique();
    }
}
