using CargoFlow.Domain.Entities.Fuelings;

namespace CargoFlow.Application.Fuelings;

public interface IFuelingRepository
{
    Task<List<Fueling>> GetAllAsync(CancellationToken cancellationToken);
    Task<Fueling?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Fueling>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken);
    Task<decimal> GetTotalCostByTripIdAsync(Guid tripId, CancellationToken cancellationToken);
    Task AddAsync(Fueling fueling, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
