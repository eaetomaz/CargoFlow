using CargoFlow.Domain.Entities.Trips;

namespace CargoFlow.Application.Trips;

public record TripProfitability(decimal Revenue, decimal TotalCost, decimal Margin, decimal? MarginPercentage, decimal? CostPerKm);

public interface ITripFinancialService
{
    TripProfitability CalculateProfitability(Trip trip, decimal fuelingCost = 0);
}

// Puro -- só soma despesas já carregadas em Trip.Expenses, mais o custo de
// abastecimento (Fuelings vinculados ao TripId, buscado à parte por
// TripService via IFuelingRepository -- Fueling não é uma coleção do
// agregado Trip). Regra 7 do documento de contexto: "custos de viagem devem
// compor sua rentabilidade".
public class TripFinancialService : ITripFinancialService
{
    public TripProfitability CalculateProfitability(Trip trip, decimal fuelingCost = 0)
    {
        var totalCost = trip.Expenses.Sum(e => e.Value) + fuelingCost;
        var margin = trip.FreightRevenue - totalCost;
        var marginPercentage = trip.FreightRevenue == 0 ? null : (decimal?)Math.Round(margin / trip.FreightRevenue * 100, 2);

        var distance = trip.ActualDistanceKm ?? trip.PlannedDistanceKm;
        var costPerKm = distance == 0 ? null : (decimal?)Math.Round(totalCost / distance, 2);

        return new TripProfitability(trip.FreightRevenue, totalCost, margin, marginPercentage, costPerKm);
    }
}
