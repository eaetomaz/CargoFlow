namespace CargoFlow.Application.Freight;

public record FreightPricingRuleDto(
    Guid Id,
    string VehicleType,
    string? CargoType,
    decimal PricePerKg,
    decimal PricePerKm,
    decimal TollPerKm,
    decimal AdValoremPercentage,
    decimal GrisPercentage,
    decimal MinimumFreightValue,
    string EffectiveFrom,
    string? EffectiveTo);

public record UpsertFreightPricingRuleRequest(
    string VehicleType,
    string? CargoType,
    decimal PricePerKg,
    decimal PricePerKm,
    decimal TollPerKm,
    decimal AdValoremPercentage,
    decimal GrisPercentage,
    decimal MinimumFreightValue,
    string EffectiveFrom,
    string? EffectiveTo);
