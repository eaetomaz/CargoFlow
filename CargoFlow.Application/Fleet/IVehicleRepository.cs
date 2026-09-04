using CargoFlow.Domain.Entities.Fleet;

namespace CargoFlow.Application.Fleet;

public interface IVehicleRepository
{
    Task<List<Vehicle>> GetAllAsync(CancellationToken cancellationToken);
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> PlateNumberExistsAsync(string plateNumber, Guid? excludingId, CancellationToken cancellationToken);
    Task<bool> RenavamExistsAsync(string renavam, Guid? excludingId, CancellationToken cancellationToken);
    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken);
    Task DeleteAsync(Vehicle vehicle, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
