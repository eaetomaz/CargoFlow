namespace CargoFlow.Application.Fuelings;

public record FuelingDto(
    Guid Id,
    Guid VehicleId,
    string VehiclePlate,
    Guid? DriverId,
    string? DriverName,
    Guid? TripId,
    string GasStationName,
    string FuelingDate,
    decimal LiterQuantity,
    decimal TotalValue,
    decimal OdometerReading);

public record CreateFuelingRequest(
    Guid VehicleId,
    Guid? DriverId,
    Guid? TripId,
    string GasStationName,
    string FuelingDate,
    decimal LiterQuantity,
    decimal TotalValue,
    decimal OdometerReading);

public record FuelEfficiencyDto(Guid VehicleId, decimal? KmPerLiter, decimal? CostPerKm);
