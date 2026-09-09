using CargoFlow.Application.Finance;
using CargoFlow.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence.Repositories.Finance;

public class AccountReceivableRepository(CargoFlowDbContext context) : IAccountReceivableRepository
{
    public Task<List<AccountReceivable>> GetAllAsync(CancellationToken cancellationToken) =>
        context.AccountsReceivable
            .Include(r => r.CustomerCompany)
            .OrderBy(r => r.DueDate)
            .ToListAsync(cancellationToken);

    public Task<AccountReceivable?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.AccountsReceivable
            .Include(r => r.CustomerCompany)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> ExistsForTripAsync(Guid tripId, CancellationToken cancellationToken) =>
        context.AccountsReceivable.AnyAsync(r => r.TripId == tripId, cancellationToken);

    public async Task AddAsync(AccountReceivable receivable, CancellationToken cancellationToken)
    {
        context.AccountsReceivable.Add(receivable);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
