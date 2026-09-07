using CargoFlow.Domain.Entities.Maintenance;

namespace CargoFlow.Application.Maintenance;

public interface IMaintenanceOrderRepository
{
    Task<List<MaintenanceOrder>> GetAllAsync(CancellationToken cancellationToken);
    Task<MaintenanceOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(MaintenanceOrder order, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
