using CargoFlow.Domain.Common;
using CargoFlow.Domain.Entities.Drivers;
using CargoFlow.Domain.Entities.Fleet;

namespace CargoFlow.Domain.Entities.Occurrences;

public enum OccurrenceType
{
    PneuFurado,
    Acidente,
    Atraso,
    Quebra,
    ClienteAusente,
    CargaAvariada,
    DocumentacaoIrregular,
}

public enum OccurrenceStatus
{
    Aberta,
    EmAndamento,
    Resolvida,
    Cancelada,
}

public class Occurrence : Entity, IAuditable
{
    public Guid? TripId { get; set; }
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public Guid DriverId { get; set; }
    public Driver? Driver { get; set; }
    public OccurrenceType Type { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ResponsibleUserId { get; set; }
    public OccurrenceStatus Status { get; set; } = OccurrenceStatus.Aberta;
    public DateTime? ResolvedAt { get; set; }

    public List<OccurrenceAttachment> Attachments { get; set; } = [];

    public void Resolve(DateTime now)
    {
        if (Status is not (OccurrenceStatus.Aberta or OccurrenceStatus.EmAndamento))
            throw new InvalidOperationException($"Só é possível resolver uma ocorrência \"Aberta\" ou \"EmAndamento\" -- esta está \"{Status}\".");

        Status = OccurrenceStatus.Resolvida;
        ResolvedAt = now;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        if (Status == OccurrenceStatus.Resolvida)
            throw new InvalidOperationException("Ocorrência resolvida não pode ser cancelada.");
        if (Status == OccurrenceStatus.Cancelada)
            return; // idempotente

        Status = OccurrenceStatus.Cancelada;
        UpdateTimestamp();
    }
}
