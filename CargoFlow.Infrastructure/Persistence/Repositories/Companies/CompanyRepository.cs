using CargoFlow.Application.Companies;
using CargoFlow.Domain.Entities.Companies;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence.Repositories.Companies;

public class CompanyRepository(CargoFlowDbContext context) : ICompanyRepository
{
    public Task<List<Company>> GetAllAsync(CancellationToken cancellationToken) =>
        context.Companies
            .Include(c => c.Addresses)
            .Include(c => c.Contacts)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Companies
            .Include(c => c.Addresses)
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> DocumentExistsAsync(string document, Guid? excludingId, CancellationToken cancellationToken) =>
        context.Companies.AnyAsync(c => c.Document == document && (excludingId == null || c.Id != excludingId), cancellationToken);

    public async Task AddAsync(Company company, CancellationToken cancellationToken)
    {
        context.Companies.Add(company);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Company company, CancellationToken cancellationToken)
    {
        context.Companies.Remove(company);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
