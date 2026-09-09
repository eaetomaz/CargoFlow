using CargoFlow.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CargoFlow.Infrastructure.Persistence.Configurations.Finance;

public class AccountPayableConfiguration : IEntityTypeConfiguration<AccountPayable>
{
    public void Configure(EntityTypeBuilder<AccountPayable> builder)
    {
        builder.HasOne(p => p.SupplierCompany)
            .WithMany()
            .HasForeignKey(p => p.SupplierCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.Amount).HasColumnType("numeric(12,2)");
        builder.HasIndex(p => p.Status);
    }
}

public class AccountReceivableConfiguration : IEntityTypeConfiguration<AccountReceivable>
{
    public void Configure(EntityTypeBuilder<AccountReceivable> builder)
    {
        builder.HasOne(r => r.CustomerCompany)
            .WithMany()
            .HasForeignKey(r => r.CustomerCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.Amount).HasColumnType("numeric(12,2)");
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.TripId);
    }
}
