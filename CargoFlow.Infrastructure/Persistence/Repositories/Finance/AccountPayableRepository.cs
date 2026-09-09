using CargoFlow.Application.Finance;
using CargoFlow.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence.Repositories.Finance;

public class AccountPayableRepository(CargoFlowDbContext context) : IAccountPayableRepository
{
    public Task<List<AccountPayable>> GetAllAsync(CancellationToken cancellationToken) =>
        context.AccountsPayable
            .Include(p => p.SupplierCompany)
            .OrderBy(p => p.DueDate)
            .ToListAsync(cancellationToken);

    public Task<AccountPayable?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.AccountsPayable
            .Include(p => p.SupplierCompany)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task AddAsync(AccountPayable payable, CancellationToken cancellationToken)
    {
        context.AccountsPayable.Add(payable);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
