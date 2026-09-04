namespace CargoFlow.Application.Drivers;

public interface IDriverService
{
    Task<List<DriverDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<DriverDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<DriverDto> CreateAsync(UpsertDriverRequest request, CancellationToken cancellationToken);
    Task<DriverDto> UpdateAsync(Guid id, UpsertDriverRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
