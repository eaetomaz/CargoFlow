using CargoFlow.Domain.Entities.Finance;

namespace CargoFlow.Application.Finance;

public interface IAccountReceivableRepository
{
    Task<List<AccountReceivable>> GetAllAsync(CancellationToken cancellationToken);
    Task<AccountReceivable?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsForTripAsync(Guid tripId, CancellationToken cancellationToken);
    Task AddAsync(AccountReceivable receivable, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
