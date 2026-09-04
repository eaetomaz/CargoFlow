using CargoFlow.Domain.Entities.Freight;

namespace CargoFlow.Application.Freight;

public interface IFreightQuoteRepository
{
    Task<List<FreightQuote>> GetAllAsync(CancellationToken cancellationToken);
    Task<FreightQuote?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(FreightQuote quote, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
