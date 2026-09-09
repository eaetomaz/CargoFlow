using CargoFlow.Application.Maintenance;
using CargoFlow.Domain.Entities.Maintenance;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence.Repositories.Maintenance;

public class MaintenanceOrderRepository(CargoFlowDbContext context) : IMaintenanceOrderRepository
{
    private static IQueryable<MaintenanceOrder> Loaded(CargoFlowDbContext ctx) =>
        ctx.MaintenanceOrders.Include(o => o.Vehicle);

    public Task<List<MaintenanceOrder>> GetAllAsync(CancellationToken cancellationToken) =>
        Loaded(context).OrderByDescending(o => o.OpenedAt).ToListAsync(cancellationToken);

    public Task<MaintenanceOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Loaded(context).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task AddAsync(MaintenanceOrder order, CancellationToken cancellationToken)
    {
        context.MaintenanceOrders.Add(order);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
