namespace CargoFlow.Application.Maintenance;

public record MaintenanceOrderDto(
    Guid Id,
    Guid VehicleId,
    string VehiclePlate,
    string Type,
    string Description,
    string Status,
    string OpenedAt,
    string? ScheduledDate,
    string? StartedAt,
    string? CompletedAt,
    decimal? OdometerAtService,
    decimal? Cost,
    string? ServiceProvider);

public record CreateMaintenanceOrderRequest(
    Guid VehicleId,
    string Type,
    string Description,
    string? ScheduledDate,
    string? ServiceProvider);

public record CompleteMaintenanceOrderRequest(decimal Cost, decimal? OdometerAtService);
