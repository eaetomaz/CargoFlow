namespace CargoFlow.Application.Freight;

public record FreightQuoteDto(
    Guid Id,
    Guid CustomerCompanyId,
    string CustomerCompanyName,
    string OriginCity,
    string OriginState,
    string DestinationCity,
    string DestinationState,
    string CargoType,
    decimal CargoWeightKg,
    decimal CargoValue,
    string RequiredVehicleType,
    decimal EstimatedDistanceKm,
    decimal FreightWeightValue,
    decimal TollValue,
    decimal AdValoremValue,
    decimal GrisValue,
    decimal OtherCostsValue,
    decimal TotalValue,
    string Status,
    string ExpiresAt,
    string CreatedAt);

public record CalculateFreightQuoteRequest(
    string CargoType,
    decimal CargoWeightKg,
    decimal CargoValue,
    string RequiredVehicleType,
    decimal EstimatedDistanceKm,
    decimal OtherCostsValue);

public record CreateFreightQuoteRequest(
    Guid CustomerCompanyId,
    string OriginCity,
    string OriginState,
    string DestinationCity,
    string DestinationState,
    string CargoType,
    decimal CargoWeightKg,
    decimal CargoValue,
    string RequiredVehicleType,
    decimal EstimatedDistanceKm,
    decimal OtherCostsValue,
    int ValidForDays);
