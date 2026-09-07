namespace CargoFlow.Application.Fuelings;

public interface IFuelingService
{
    Task<List<FuelingDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<FuelingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<FuelingDto> CreateAsync(CreateFuelingRequest request, CancellationToken cancellationToken);
    Task<FuelEfficiencyDto> GetFuelEfficiencyAsync(Guid vehicleId, CancellationToken cancellationToken);
}
