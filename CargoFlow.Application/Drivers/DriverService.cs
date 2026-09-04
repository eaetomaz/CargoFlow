using CargoFlow.Domain.Entities.Drivers;

namespace CargoFlow.Application.Drivers;

public class DriverService(IDriverRepository repository) : IDriverService
{
    public async Task<List<DriverDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var drivers = await repository.GetAllAsync(cancellationToken);
        return drivers.Select(ToDto).ToList();
    }

    public async Task<DriverDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var driver = await repository.GetByIdAsync(id, cancellationToken);
        return driver is null ? null : ToDto(driver);
    }

    public async Task<DriverDto> CreateAsync(UpsertDriverRequest request, CancellationToken cancellationToken)
    {
        if (await repository.CpfExistsAsync(request.Cpf, null, cancellationToken))
            throw new InvalidOperationException($"Já existe um motorista cadastrado com o CPF {request.Cpf}.");

        if (await repository.CnhNumberExistsAsync(request.CnhNumber, null, cancellationToken))
            throw new InvalidOperationException($"Já existe um motorista cadastrado com a CNH {request.CnhNumber}.");

        var driver = new Driver();
        Apply(driver, request);

        await repository.AddAsync(driver, cancellationToken);
        return ToDto(driver);
    }

    public async Task<DriverDto> UpdateAsync(Guid id, UpsertDriverRequest request, CancellationToken cancellationToken)
    {
        var driver = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Motorista não encontrado.");

        if (await repository.CpfExistsAsync(request.Cpf, id, cancellationToken))
            throw new InvalidOperationException($"Já existe outro motorista cadastrado com o CPF {request.Cpf}.");

        if (await repository.CnhNumberExistsAsync(request.CnhNumber, id, cancellationToken))
            throw new InvalidOperationException($"Já existe outro motorista cadastrado com a CNH {request.CnhNumber}.");

        Apply(driver, request);
        driver.UpdateTimestamp();

        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(driver);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var driver = await repository.GetByIdAsync(id, cancellationToken);
        if (driver is null)
            return; // idempotente

        await repository.DeleteAsync(driver, cancellationToken);
    }

    private static void Apply(Driver driver, UpsertDriverRequest request)
    {
        driver.Name = request.Name.Trim();
        driver.Cpf = request.Cpf.Trim();
        driver.BirthDate = request.BirthDate;
        driver.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        driver.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        driver.CnhNumber = request.CnhNumber.Trim();
        driver.CnhCategory = Enum.Parse<CnhCategory>(request.CnhCategory);
        driver.CnhExpiryDate = request.CnhExpiryDate;
        driver.HireDate = request.HireDate;
        driver.EmploymentType = Enum.Parse<EmploymentType>(request.EmploymentType);
        driver.AvailabilityStatus = Enum.Parse<DriverAvailabilityStatus>(request.AvailabilityStatus);
    }

    private static DriverDto ToDto(Driver d) => new(
        d.Id,
        d.Name,
        d.Cpf,
        d.BirthDate,
        d.Phone,
        d.Email,
        d.CnhNumber,
        d.CnhCategory.ToString(),
        d.CnhExpiryDate,
        d.HireDate,
        d.EmploymentType.ToString(),
        d.AvailabilityStatus.ToString());
}
