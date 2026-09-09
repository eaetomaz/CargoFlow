using CargoFlow.Domain.Entities.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CargoFlow.Infrastructure.Persistence.Configurations.Documents;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.Property(d => d.Number).HasMaxLength(50).IsRequired();

        // Consulta mais comum: "documentos de tal dono" e "documentos por
        // status pra alerta" -- os dois viram índice.
        builder.HasIndex(d => new { d.OwnerType, d.OwnerId });
        builder.HasIndex(d => d.Status);
    }
}
