using CargoFlow.Application.Fuelings;
using CargoFlow.Domain.Entities.Fuelings;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence.Repositories.Fuelings;

public class FuelingRepository(CargoFlowDbContext context) : IFuelingRepository
{
    private static IQueryable<Fueling> Loaded(CargoFlowDbContext ctx) =>
        ctx.Fuelings
            .Include(f => f.Vehicle)
            .Include(f => f.Driver);

    public Task<List<Fueling>> GetAllAsync(CancellationToken cancellationToken) =>
        Loaded(context).OrderByDescending(f => f.FuelingDate).ToListAsync(cancellationToken);

    public Task<Fueling?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Loaded(context).FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public Task<List<Fueling>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken) =>
        context.Fuelings.Where(f => f.VehicleId == vehicleId).ToListAsync(cancellationToken);

    public Task<decimal> GetTotalCostByTripIdAsync(Guid tripId, CancellationToken cancellationToken) =>
        context.Fuelings.Where(f => f.TripId == tripId).SumAsync(f => f.TotalValue, cancellationToken);

    public async Task AddAsync(Fueling fueling, CancellationToken cancellationToken)
    {
        context.Fuelings.Add(fueling);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
