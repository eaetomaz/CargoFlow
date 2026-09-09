using CargoFlow.Domain.Entities.Occurrences;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CargoFlow.Infrastructure.Persistence.Configurations.Occurrences;

public class OccurrenceConfiguration : IEntityTypeConfiguration<Occurrence>
{
    public void Configure(EntityTypeBuilder<Occurrence> builder)
    {
        builder.Property(o => o.Location).HasMaxLength(200).IsRequired();
        builder.Property(o => o.Description).HasMaxLength(2000).IsRequired();

        builder.HasOne(o => o.Vehicle)
            .WithMany()
            .HasForeignKey(o => o.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Driver)
            .WithMany()
            .HasForeignKey(o => o.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Attachments)
            .WithOne(a => a.Occurrence)
            .HasForeignKey(a => a.OccurrenceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.TripId);
    }
}

public class OccurrenceAttachmentConfiguration : IEntityTypeConfiguration<OccurrenceAttachment>
{
    public void Configure(EntityTypeBuilder<OccurrenceAttachment> builder)
    {
        builder.Property(a => a.FileUrl).HasMaxLength(1000).IsRequired();
        builder.Property(a => a.FileName).HasMaxLength(300).IsRequired();
    }
}
