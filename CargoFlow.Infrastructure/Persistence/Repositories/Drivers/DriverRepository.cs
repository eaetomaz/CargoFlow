using CargoFlow.Application.Drivers;
using CargoFlow.Domain.Entities.Drivers;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence.Repositories.Drivers;

public class DriverRepository(CargoFlowDbContext context) : IDriverRepository
{
    public Task<List<Driver>> GetAllAsync(CancellationToken cancellationToken) =>
        context.Drivers.OrderBy(d => d.Name).ToListAsync(cancellationToken);

    public Task<Driver?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Drivers.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<bool> CpfExistsAsync(string cpf, Guid? excludingId, CancellationToken cancellationToken) =>
        context.Drivers.AnyAsync(d => d.Cpf == cpf && (excludingId == null || d.Id != excludingId), cancellationToken);

    public Task<bool> CnhNumberExistsAsync(string cnhNumber, Guid? excludingId, CancellationToken cancellationToken) =>
        context.Drivers.AnyAsync(d => d.CnhNumber == cnhNumber && (excludingId == null || d.Id != excludingId), cancellationToken);

    public async Task AddAsync(Driver driver, CancellationToken cancellationToken)
    {
        context.Drivers.Add(driver);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Driver driver, CancellationToken cancellationToken)
    {
        context.Drivers.Remove(driver);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
