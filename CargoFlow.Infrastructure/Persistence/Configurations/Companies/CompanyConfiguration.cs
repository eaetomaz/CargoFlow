using CargoFlow.Domain.Entities.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CargoFlow.Infrastructure.Persistence.Configurations.Companies;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Document).HasMaxLength(20).IsRequired();
        builder.HasIndex(c => c.Document).IsUnique();

        builder.HasMany(c => c.Addresses)
            .WithOne(a => a.Company)
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Contacts)
            .WithOne(a => a.Company)
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
