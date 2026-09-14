using CargoFlow.Application.Freight;
using CargoFlow.Domain.Entities.Fleet;
using CargoFlow.Domain.Entities.Freight;

namespace CargoFlow.Application.Tests.Freight;

public class FreightQuoteCalculationServiceTests
{
    private readonly FreightQuoteCalculationService _sut = new();

    private static FreightPricingRule Rule(
        decimal pricePerKg = 0.5m,
        decimal pricePerKm = 2m,
        decimal tollPerKm = 0.1m,
        decimal adValoremPct = 1m,
        decimal grisPct = 0.5m,
        decimal minimum = 100m) => new()
        {
            VehicleType = VehicleType.CaminhaoToco,
            PricePerKg = pricePerKg,
            PricePerKm = pricePerKm,
            TollPerKm = tollPerKm,
            AdValoremPercentage = adValoremPct,
            GrisPercentage = grisPct,
            MinimumFreightValue = minimum,
        };

    [Fact]
    public void Calculate_ComponentsSumToTotal_WhenAboveMinimum()
    {
        var rule = Rule();
        var result = _sut.Calculate(rule, cargoWeightKg: 1000, cargoValue: 10000, estimatedDistanceKm: 200, otherCostsValue: 50);

        var expectedFreightWeight = 1000 * 0.5m + 200 * 2m; // 500 + 400 = 900
        var expectedToll = 200 * 0.1m; // 20
        var expectedAdValorem = 10000 * 1m / 100m; // 100
        var expectedGris = 10000 * 0.5m / 100m; // 50
        var expectedTotal = expectedFreightWeight + expectedToll + expectedAdValorem + expectedGris + 50; // + otherCosts

        Assert.Equal(expectedFreightWeight, result.FreightWeightValue);
        Assert.Equal(expectedToll, result.TollValue);
        Assert.Equal(expectedAdValorem, result.AdValoremValue);
        Assert.Equal(expectedGris, result.GrisValue);
        Assert.Equal(50m, result.OtherCostsValue);
        Assert.Equal(expectedTotal, result.TotalValue);
    }

    [Fact]
    public void Calculate_AppliesMinimumFreightValue_WhenComponentsSumBelowIt()
    {
        var rule = Rule(pricePerKg: 0.01m, pricePerKm: 0.01m, tollPerKm: 0m, adValoremPct: 0m, grisPct: 0m, minimum: 500m);
        var result = _sut.Calculate(rule, cargoWeightKg: 10, cargoValue: 100, estimatedDistanceKm: 5, otherCostsValue: 0);

        // 10*0.01 + 5*0.01 = 0.15, muito abaixo do mínimo de 500
        Assert.Equal(500m, result.TotalValue);
    }

    [Fact]
    public void Calculate_ZeroDistanceAndWeight_DoesNotThrow_AndUsesMinimum()
    {
        var rule = Rule(minimum: 80m);
        var result = _sut.Calculate(rule, cargoWeightKg: 0, cargoValue: 0, estimatedDistanceKm: 0, otherCostsValue: 0);

        Assert.Equal(0m, result.FreightWeightValue);
        Assert.Equal(0m, result.TollValue);
        Assert.Equal(80m, result.TotalValue);
    }

    [Fact]
    public void Calculate_EachPricingRuleField_IsAppliedIndependently()
    {
        var baseline = _sut.Calculate(Rule(), cargoWeightKg: 100, cargoValue: 1000, estimatedDistanceKm: 100, otherCostsValue: 0);
        var higherToll = _sut.Calculate(Rule(tollPerKm: 1m), cargoWeightKg: 100, cargoValue: 1000, estimatedDistanceKm: 100, otherCostsValue: 0);

        Assert.True(higherToll.TollValue > baseline.TollValue);
        Assert.Equal(baseline.FreightWeightValue, higherToll.FreightWeightValue);
        Assert.Equal(baseline.AdValoremValue, higherToll.AdValoremValue);
        Assert.Equal(baseline.GrisValue, higherToll.GrisValue);
    }
}
