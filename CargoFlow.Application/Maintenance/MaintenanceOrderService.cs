using CargoFlow.Application.Fleet;
using CargoFlow.Domain.Entities.Fleet;
using CargoFlow.Domain.Entities.Maintenance;

namespace CargoFlow.Application.Maintenance;

public class MaintenanceOrderService(
    IMaintenanceOrderRepository repository,
    IVehicleRepository vehicleRepository) : IMaintenanceOrderService
{
    public async Task<List<MaintenanceOrderDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var orders = await repository.GetAllAsync(cancellationToken);
        return orders.Select(ToDto).ToList();
    }

    public async Task<MaintenanceOrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : ToDto(order);
    }

    // Um veículo com ordem de manutenção aberta não pode ser escalado --
    // liga de volta na regra 2 (TripSchedulingRules.ValidateVehicleEligibility
    // já exige Status == Disponivel).
    public async Task<MaintenanceOrderDto> CreateAsync(CreateMaintenanceOrderRequest request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Veículo não encontrado.");

        var order = new MaintenanceOrder
        {
            VehicleId = request.VehicleId,
            Vehicle = vehicle,
            Type = Enum.Parse<MaintenanceType>(request.Type),
            Description = request.Description,
            OpenedAt = DateTime.UtcNow,
            ScheduledDate = string.IsNullOrWhiteSpace(request.ScheduledDate) ? null : DateOnly.Parse(request.ScheduledDate),
            ServiceProvider = request.ServiceProvider,
        };

        vehicle.Status = VehicleStatus.EmManutencao;

        await repository.AddAsync(order, cancellationToken);
        return ToDto(order);
    }

    public async Task<MaintenanceOrderDto> StartAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await GetOrThrowAsync(id, cancellationToken);
        order.Start(DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<MaintenanceOrderDto> CompleteAsync(Guid id, CompleteMaintenanceOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await GetOrThrowAsync(id, cancellationToken);
        order.Complete(DateTime.UtcNow, request.Cost, request.OdometerAtService);

        var vehicle = await vehicleRepository.GetByIdAsync(order.VehicleId, cancellationToken);
        // Defensivo: só libera se ainda estiver EmManutencao -- evita
        // sobrescrever um status que tenha mudado por outro caminho
        // (ex: veículo escalado em outra viagem nesse meio-tempo).
        if (vehicle is not null && vehicle.Status == VehicleStatus.EmManutencao)
            vehicle.Status = VehicleStatus.Disponivel;

        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<MaintenanceOrderDto> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await GetOrThrowAsync(id, cancellationToken);
        order.Cancel();
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    private async Task<MaintenanceOrder> GetOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Ordem de manutenção não encontrada.");

    private static MaintenanceOrderDto ToDto(MaintenanceOrder o) => new(
        o.Id,
        o.VehicleId,
        o.Vehicle?.PlateNumber ?? string.Empty,
        o.Type.ToString(),
        o.Description,
        o.Status.ToString(),
        o.OpenedAt.ToString("O"),
        o.ScheduledDate?.ToString("yyyy-MM-dd"),
        o.StartedAt?.ToString("O"),
        o.CompletedAt?.ToString("O"),
        o.OdometerAtService,
        o.Cost,
        o.ServiceProvider);
}
