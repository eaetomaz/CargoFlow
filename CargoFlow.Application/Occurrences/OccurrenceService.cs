using CargoFlow.Application.Drivers;
using CargoFlow.Application.Fleet;
using CargoFlow.Application.Trips;
using CargoFlow.Domain.Entities.Fleet;
using CargoFlow.Domain.Entities.Occurrences;
using CargoFlow.Domain.Entities.Trips;

namespace CargoFlow.Application.Occurrences;

public class OccurrenceService(
    IOccurrenceRepository repository,
    IVehicleRepository vehicleRepository,
    IDriverRepository driverRepository,
    ITripRepository tripRepository) : IOccurrenceService
{
    public async Task<List<OccurrenceDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var occurrences = await repository.GetAllAsync(cancellationToken);
        return occurrences.Select(ToDto).ToList();
    }

    public async Task<OccurrenceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var occurrence = await repository.GetByIdAsync(id, cancellationToken);
        return occurrence is null ? null : ToDto(occurrence);
    }

    // Regra do documento de contexto: algumas ocorrências disparam workflows
    // automáticos -- uma Quebra torna o veículo indisponível pra escalar
    // (EmManutencao), e toda ocorrência vinculada a uma viagem entra na
    // timeline dela (TripEventType.OccurrenceCreated).
    public async Task<OccurrenceDto> CreateAsync(CreateOccurrenceRequest request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Veículo não encontrado.");
        var driver = await driverRepository.GetByIdAsync(request.DriverId, cancellationToken)
            ?? throw new InvalidOperationException("Motorista não encontrado.");

        var type = Enum.Parse<OccurrenceType>(request.Type);
        var occurredAt = DateTime.Parse(request.OccurredAt, null, System.Globalization.DateTimeStyles.RoundtripKind);

        var occurrence = new Occurrence
        {
            TripId = request.TripId,
            VehicleId = request.VehicleId,
            Vehicle = vehicle,
            DriverId = request.DriverId,
            Driver = driver,
            Type = type,
            OccurredAt = occurredAt,
            Location = request.Location,
            Description = request.Description,
            ResponsibleUserId = request.ResponsibleUserId,
        };

        if (type == OccurrenceType.Quebra)
            vehicle.Status = VehicleStatus.EmManutencao;

        await repository.AddAsync(occurrence, cancellationToken);

        if (request.TripId is { } tripId)
        {
            var trip = await tripRepository.GetByIdAsync(tripId, cancellationToken)
                ?? throw new InvalidOperationException("Viagem vinculada não encontrada.");

            trip.AppendEvent(TripEventType.OccurrenceCreated, occurredAt, $"Ocorrência registrada: {type} -- {request.Description}");
            await tripRepository.SaveChangesAsync(cancellationToken);
        }

        return ToDto(occurrence);
    }

    public async Task<OccurrenceDto> ResolveAsync(Guid id, CancellationToken cancellationToken)
    {
        var occurrence = await GetOrThrowAsync(id, cancellationToken);
        occurrence.Resolve(DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(occurrence);
    }

    public async Task<OccurrenceDto> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var occurrence = await GetOrThrowAsync(id, cancellationToken);
        occurrence.Cancel();
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(occurrence);
    }

    public async Task<OccurrenceDto> AddAttachmentAsync(Guid id, AddOccurrenceAttachmentRequest request, CancellationToken cancellationToken)
    {
        var occurrence = await GetOrThrowAsync(id, cancellationToken);
        occurrence.Attachments.Add(new OccurrenceAttachment
        {
            OccurrenceId = occurrence.Id,
            FileUrl = request.FileUrl,
            FileName = request.FileName,
            UploadedAt = DateTime.UtcNow,
        });
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(occurrence);
    }

    private async Task<Occurrence> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Ocorrência não encontrada.");

    private static OccurrenceDto ToDto(Occurrence o) => new(
        o.Id,
        o.TripId,
        o.VehicleId,
        o.Vehicle?.PlateNumber ?? string.Empty,
        o.DriverId,
        o.Driver?.Name ?? string.Empty,
        o.Type.ToString(),
        o.OccurredAt.ToString("O"),
        o.Location,
        o.Description,
        o.ResponsibleUserId,
        o.Status.ToString(),
        o.ResolvedAt?.ToString("O"),
        o.Attachments.Select(a => new OccurrenceAttachmentDto(a.Id, a.FileUrl, a.FileName, a.UploadedAt.ToString("O"))).ToList());
}
