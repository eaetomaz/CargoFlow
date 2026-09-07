namespace CargoFlow.Application.Occurrences;

public interface IOccurrenceService
{
    Task<List<OccurrenceDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<OccurrenceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<OccurrenceDto> CreateAsync(CreateOccurrenceRequest request, CancellationToken cancellationToken);
    Task<OccurrenceDto> ResolveAsync(Guid id, CancellationToken cancellationToken);
    Task<OccurrenceDto> CancelAsync(Guid id, CancellationToken cancellationToken);
    Task<OccurrenceDto> AddAttachmentAsync(Guid id, AddOccurrenceAttachmentRequest request, CancellationToken cancellationToken);
}
