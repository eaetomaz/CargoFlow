using CargoFlow.Domain.Entities.Companies;

namespace CargoFlow.Application.Companies;

public class CompanyService(ICompanyRepository repository) : ICompanyService
{
    public async Task<List<CompanyDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var companies = await repository.GetAllAsync(cancellationToken);
        return companies.Select(ToDto).ToList();
    }

    public async Task<CompanyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var company = await repository.GetByIdAsync(id, cancellationToken);
        return company is null ? null : ToDto(company);
    }

    public async Task<CompanyDto> CreateAsync(UpsertCompanyRequest request, CancellationToken cancellationToken)
    {
        if (await repository.DocumentExistsAsync(request.Document, null, cancellationToken))
            throw new InvalidOperationException($"Já existe uma empresa cadastrada com o documento {request.Document}.");

        var company = new Company();
        Apply(company, request);

        await repository.AddAsync(company, cancellationToken);
        return ToDto(company);
    }

    public async Task<CompanyDto> UpdateAsync(Guid id, UpsertCompanyRequest request, CancellationToken cancellationToken)
    {
        var company = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Empresa não encontrada.");

        if (await repository.DocumentExistsAsync(request.Document, id, cancellationToken))
            throw new InvalidOperationException($"Já existe outra empresa cadastrada com o documento {request.Document}.");

        Apply(company, request);
        company.UpdateTimestamp();

        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(company);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var company = await repository.GetByIdAsync(id, cancellationToken);
        if (company is null)
            return; // idempotente

        await repository.DeleteAsync(company, cancellationToken);
    }

    private static void Apply(Company company, UpsertCompanyRequest request)
    {
        company.Name = request.Name.Trim();
        company.TradeName = string.IsNullOrWhiteSpace(request.TradeName) ? null : request.TradeName.Trim();
        company.Document = request.Document.Trim();
        company.DocumentType = Enum.Parse<CompanyDocumentType>(request.DocumentType);
        company.StateRegistration = string.IsNullOrWhiteSpace(request.StateRegistration) ? null : request.StateRegistration.Trim();
        company.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        company.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        company.IsActive = request.IsActive;

        company.Roles = request.Roles
            .Select(Enum.Parse<CompanyRoles>)
            .Aggregate(CompanyRoles.None, (acc, role) => acc | role);

        company.Addresses.Clear();
        company.Addresses.AddRange(request.Addresses.Select(a => new Address
        {
            Type = Enum.Parse<AddressType>(a.Type),
            Street = a.Street,
            Number = a.Number,
            Complement = a.Complement,
            District = a.District,
            City = a.City,
            State = a.State,
            ZipCode = a.ZipCode,
            IsDefault = a.IsDefault,
        }));

        company.Contacts.Clear();
        company.Contacts.AddRange(request.Contacts.Select(c => new Contact
        {
            Name = c.Name,
            Role = c.Role,
            Email = c.Email,
            Phone = c.Phone,
        }));
    }

    private static CompanyDto ToDto(Company c) => new(
        c.Id,
        c.Name,
        c.TradeName,
        c.Document,
        c.DocumentType.ToString(),
        c.StateRegistration,
        c.Email,
        c.Phone,
        SplitRoles(c.Roles),
        c.IsActive,
        c.Addresses.Select(a => new AddressDto(a.Id, a.Type.ToString(), a.Street, a.Number, a.Complement, a.District, a.City, a.State, a.ZipCode, a.IsDefault)).ToList(),
        c.Contacts.Select(ct => new ContactDto(ct.Id, ct.Name, ct.Role, ct.Email, ct.Phone)).ToList());

    private static string[] SplitRoles(CompanyRoles roles) =>
        Enum.GetValues<CompanyRoles>()
            .Where(r => r != CompanyRoles.None && roles.HasFlag(r))
            .Select(r => r.ToString())
            .ToArray();
}
