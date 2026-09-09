using CargoFlow.Application.Freight;
using CargoFlow.Domain.Entities.Freight;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence.Repositories.Freight;

public class FreightQuoteRepository(CargoFlowDbContext context) : IFreightQuoteRepository
{
    public Task<List<FreightQuote>> GetAllAsync(CancellationToken cancellationToken) =>
        context.FreightQuotes
            .Include(q => q.CustomerCompany)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<FreightQuote?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.FreightQuotes
            .Include(q => q.CustomerCompany)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public async Task AddAsync(FreightQuote quote, CancellationToken cancellationToken)
    {
        context.FreightQuotes.Add(quote);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
