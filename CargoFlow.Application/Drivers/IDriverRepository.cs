using CargoFlow.Domain.Entities.Drivers;

namespace CargoFlow.Application.Drivers;

public interface IDriverRepository
{
    Task<List<Driver>> GetAllAsync(CancellationToken cancellationToken);
    Task<Driver?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> CpfExistsAsync(string cpf, Guid? excludingId, CancellationToken cancellationToken);
    Task<bool> CnhNumberExistsAsync(string cnhNumber, Guid? excludingId, CancellationToken cancellationToken);
    Task AddAsync(Driver driver, CancellationToken cancellationToken);
    Task DeleteAsync(Driver driver, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
