using CargoFlow.Domain.Entities.Trips;

namespace CargoFlow.Application.Trips;

public interface ITripRepository
{
    Task<List<Trip>> GetAllAsync(CancellationToken cancellationToken);
    Task<Trip?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Trip trip, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
