using CargoFlow.Domain.Entities.TransportOrders;

namespace CargoFlow.Application.TransportOrders;

public interface ITransportOrderRepository
{
    Task<List<TransportOrder>> GetAllAsync(CancellationToken cancellationToken);
    Task<TransportOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(TransportOrder order, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
