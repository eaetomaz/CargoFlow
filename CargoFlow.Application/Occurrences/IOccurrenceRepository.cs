using CargoFlow.Domain.Entities.Occurrences;

namespace CargoFlow.Application.Occurrences;

public interface IOccurrenceRepository
{
    Task<List<Occurrence>> GetAllAsync(CancellationToken cancellationToken);
    Task<Occurrence?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Occurrence occurrence, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
