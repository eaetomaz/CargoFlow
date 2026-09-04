using CargoFlow.Domain.Entities.Freight;

namespace CargoFlow.Application.Freight;

public record FreightQuoteBreakdown(
    decimal FreightWeightValue,
    decimal TollValue,
    decimal AdValoremValue,
    decimal GrisValue,
    decimal OtherCostsValue,
    decimal TotalValue);

public interface IFreightQuoteCalculationService
{
    FreightQuoteBreakdown Calculate(FreightPricingRule rule, decimal cargoWeightKg, decimal cargoValue, decimal estimatedDistanceKm, decimal otherCostsValue);
}

// Pura -- nenhuma dependência de banco/repositório, só a regra de preço já
// resolvida e os números da cotação. Fácil de testar unitariamente (ver
// FreightQuoteCalculationServiceTests) e é o único lugar que soma o
// breakdown, evitando fórmulas divergentes entre criação e exibição.
public class FreightQuoteCalculationService : IFreightQuoteCalculationService
{
    public FreightQuoteBreakdown Calculate(FreightPricingRule rule, decimal cargoWeightKg, decimal cargoValue, decimal estimatedDistanceKm, decimal otherCostsValue)
    {
        var freightWeightValue = Math.Round(cargoWeightKg * rule.PricePerKg + estimatedDistanceKm * rule.PricePerKm, 2);
        var tollValue = Math.Round(estimatedDistanceKm * rule.TollPerKm, 2);
        var adValoremValue = Math.Round(cargoValue * rule.AdValoremPercentage / 100m, 2);
        var grisValue = Math.Round(cargoValue * rule.GrisPercentage / 100m, 2);

        var subtotal = freightWeightValue + tollValue + adValoremValue + grisValue + otherCostsValue;
        var totalValue = Math.Max(subtotal, rule.MinimumFreightValue);

        return new FreightQuoteBreakdown(freightWeightValue, tollValue, adValoremValue, grisValue, otherCostsValue, totalValue);
    }
}
