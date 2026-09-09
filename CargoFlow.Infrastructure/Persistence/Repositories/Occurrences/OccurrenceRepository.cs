using CargoFlow.Application.Occurrences;
using CargoFlow.Domain.Entities.Occurrences;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence.Repositories.Occurrences;

public class OccurrenceRepository(CargoFlowDbContext context) : IOccurrenceRepository
{
    private static IQueryable<Occurrence> Loaded(CargoFlowDbContext ctx) =>
        ctx.Occurrences
            .Include(o => o.Vehicle)
            .Include(o => o.Driver)
            .Include(o => o.Attachments);

    public Task<List<Occurrence>> GetAllAsync(CancellationToken cancellationToken) =>
        Loaded(context).OrderByDescending(o => o.OccurredAt).ToListAsync(cancellationToken);

    public Task<Occurrence?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Loaded(context).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task AddAsync(Occurrence occurrence, CancellationToken cancellationToken)
    {
        context.Occurrences.Add(occurrence);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
