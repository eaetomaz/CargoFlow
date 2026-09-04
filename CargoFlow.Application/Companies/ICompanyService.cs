namespace CargoFlow.Application.Companies;

public interface ICompanyService
{
    Task<List<CompanyDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<CompanyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CompanyDto> CreateAsync(UpsertCompanyRequest request, CancellationToken cancellationToken);
    Task<CompanyDto> UpdateAsync(Guid id, UpsertCompanyRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
