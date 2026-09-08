using CargoFlow.Domain.Entities.Finance;

namespace CargoFlow.Application.Finance;

public interface IAccountPayableRepository
{
    Task<List<AccountPayable>> GetAllAsync(CancellationToken cancellationToken);
    Task<AccountPayable?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(AccountPayable payable, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
