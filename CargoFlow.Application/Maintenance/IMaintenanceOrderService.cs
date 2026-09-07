namespace CargoFlow.Application.Maintenance;

public interface IMaintenanceOrderService
{
    Task<List<MaintenanceOrderDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<MaintenanceOrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<MaintenanceOrderDto> CreateAsync(CreateMaintenanceOrderRequest request, CancellationToken cancellationToken);
    Task<MaintenanceOrderDto> StartAsync(Guid id, CancellationToken cancellationToken);
    Task<MaintenanceOrderDto> CompleteAsync(Guid id, CompleteMaintenanceOrderRequest request, CancellationToken cancellationToken);
    Task<MaintenanceOrderDto> CancelAsync(Guid id, CancellationToken cancellationToken);
}
