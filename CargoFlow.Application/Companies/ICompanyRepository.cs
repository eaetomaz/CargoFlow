using CargoFlow.Domain.Entities.Companies;

namespace CargoFlow.Application.Companies;

public interface ICompanyRepository
{
    Task<List<Company>> GetAllAsync(CancellationToken cancellationToken);
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DocumentExistsAsync(string document, Guid? excludingId, CancellationToken cancellationToken);
    Task AddAsync(Company company, CancellationToken cancellationToken);
    Task DeleteAsync(Company company, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
