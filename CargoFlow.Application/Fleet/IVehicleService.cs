namespace CargoFlow.Application.Fleet;

public interface IVehicleService
{
    Task<List<VehicleDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<VehicleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<VehicleDto> CreateAsync(UpsertVehicleRequest request, CancellationToken cancellationToken);
    Task<VehicleDto> UpdateAsync(Guid id, UpsertVehicleRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
