using CargoFlow.Application.Trips;
using CargoFlow.Domain.Entities.Trips;

namespace CargoFlow.Application.Tests.Trips;

public class TripFinancialServiceTests
{
    private readonly TripFinancialService _sut = new();

    private static Trip NewTrip(decimal freightRevenue, decimal plannedDistanceKm) =>
        Trip.Schedule(
            Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(),
            "Origem", "OR", "Destino", "DE",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(1), plannedDistanceKm, freightRevenue);

    [Fact]
    public void CalculateProfitability_MarginAndCostPerKm_MatchExpectedFormulas()
    {
        var trip = NewTrip(freightRevenue: 5000m, plannedDistanceKm: 500m);
        trip.AddExpense(new TripExpense { Type = TripExpenseType.Pedagio, Value = 200m, ExpenseDate = DateOnly.FromDateTime(DateTime.UtcNow) });
        trip.AddExpense(new TripExpense { Type = TripExpenseType.Diaria, Value = 300m, ExpenseDate = DateOnly.FromDateTime(DateTime.UtcNow) });

        var result = _sut.CalculateProfitability(trip);

        Assert.Equal(5000m, result.Revenue);
        Assert.Equal(500m, result.TotalCost);
        Assert.Equal(4500m, result.Margin);
        Assert.Equal(90m, result.MarginPercentage); // 4500/5000 * 100
        Assert.Equal(1m, result.CostPerKm); // 500/500
    }

    [Fact]
    public void CalculateProfitability_UsesActualDistance_WhenTripCompleted()
    {
        var trip = NewTrip(freightRevenue: 1000m, plannedDistanceKm: 100m);
        trip.AddExpense(new TripExpense { Type = TripExpenseType.Diversos, Value = 100m, ExpenseDate = DateOnly.FromDateTime(DateTime.UtcNow) });
        trip.Start(DateTime.UtcNow);
        trip.CompleteDelivery(DateTime.UtcNow, actualDistanceKm: 120m);

        var result = _sut.CalculateProfitability(trip);

        // CostPerKm é arredondado em 2 casas pelo serviço -- 100/120 = 0.8333...
        Assert.Equal(0.83m, result.CostPerKm);
    }

    [Fact]
    public void CalculateProfitability_ZeroDistance_DoesNotThrow_AndReturnsNullCostPerKm()
    {
        var trip = NewTrip(freightRevenue: 1000m, plannedDistanceKm: 0m);

        var result = _sut.CalculateProfitability(trip);

        Assert.Null(result.CostPerKm);
        Assert.Equal(1000m, result.Margin);
    }

    [Fact]
    public void CalculateProfitability_ZeroRevenue_DoesNotThrow_AndReturnsNullMarginPercentage()
    {
        var trip = NewTrip(freightRevenue: 0m, plannedDistanceKm: 100m);

        var result = _sut.CalculateProfitability(trip);

        Assert.Null(result.MarginPercentage);
    }

    [Fact]
    public void CalculateProfitability_NoExpenses_TotalCostIsZero()
    {
        var trip = NewTrip(freightRevenue: 800m, plannedDistanceKm: 200m);

        var result = _sut.CalculateProfitability(trip);

        Assert.Equal(0m, result.TotalCost);
        Assert.Equal(800m, result.Margin);
    }

    [Fact]
    public void CalculateProfitability_IncludesFuelingCost_InTotalCost()
    {
        var trip = NewTrip(freightRevenue: 5000m, plannedDistanceKm: 500m);
        trip.AddExpense(new TripExpense { Type = TripExpenseType.Pedagio, Value = 200m, ExpenseDate = DateOnly.FromDateTime(DateTime.UtcNow) });

        var result = _sut.CalculateProfitability(trip, fuelingCost: 600m);

        Assert.Equal(800m, result.TotalCost); // 200 (despesa) + 600 (combustível)
        Assert.Equal(4200m, result.Margin); // 5000 - 800
    }
}
