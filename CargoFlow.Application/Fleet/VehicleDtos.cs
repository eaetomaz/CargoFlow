namespace CargoFlow.Application.Fleet;

public record VehicleDto(
    Guid Id,
    string PlateNumber,
    string Renavam,
    string Brand,
    string Model,
    int ManufactureYear,
    int ModelYear,
    string Type,
    int AxleCount,
    decimal CapacityKg,
    decimal TareWeightKg,
    decimal Odometer,
    string Status);

public record UpsertVehicleRequest(
    string PlateNumber,
    string Renavam,
    string Brand,
    string Model,
    int ManufactureYear,
    int ModelYear,
    string Type,
    int AxleCount,
    decimal CapacityKg,
    decimal TareWeightKg,
    decimal Odometer,
    string Status);
