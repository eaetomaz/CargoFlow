using CargoFlow.Application.Trips;
using CargoFlow.Domain.Entities.Trips;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence.Repositories.Trips;

public class TripRepository(CargoFlowDbContext context) : ITripRepository
{
    private static IQueryable<Trip> Loaded(CargoFlowDbContext ctx) =>
        ctx.Trips
            .Include(t => t.Vehicle)
            .Include(t => t.Driver)
            .Include(t => t.Expenses)
            .Include(t => t.Events);

    public Task<List<Trip>> GetAllAsync(CancellationToken cancellationToken) =>
        Loaded(context).OrderByDescending(t => t.ScheduledDepartureAt).ToListAsync(cancellationToken);

    public Task<Trip?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Loaded(context).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task AddAsync(Trip trip, CancellationToken cancellationToken)
    {
        context.Trips.Add(trip);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
