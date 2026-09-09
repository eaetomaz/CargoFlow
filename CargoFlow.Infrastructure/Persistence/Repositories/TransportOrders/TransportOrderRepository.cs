using CargoFlow.Application.TransportOrders;
using CargoFlow.Domain.Entities.TransportOrders;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence.Repositories.TransportOrders;

public class TransportOrderRepository(CargoFlowDbContext context) : ITransportOrderRepository
{
    public Task<List<TransportOrder>> GetAllAsync(CancellationToken cancellationToken) =>
        context.TransportOrders
            .Include(o => o.CustomerCompany)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<TransportOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.TransportOrders
            .Include(o => o.CustomerCompany)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task AddAsync(TransportOrder order, CancellationToken cancellationToken)
    {
        context.TransportOrders.Add(order);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
