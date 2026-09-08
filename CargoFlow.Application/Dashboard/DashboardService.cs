using CargoFlow.Application.Documents;
using CargoFlow.Application.Fleet;
using CargoFlow.Application.Fuelings;
using CargoFlow.Application.Occurrences;
using CargoFlow.Application.Trips;
using CargoFlow.Domain.Entities.Documents;
using CargoFlow.Domain.Entities.Fleet;
using CargoFlow.Domain.Entities.Occurrences;
using CargoFlow.Domain.Entities.Trips;

namespace CargoFlow.Application.Dashboard;

// Carrega listas completas via os repositórios já existentes (GetAllAsync)
// e agrega tudo em memória -- aceitável pra escala de um projeto de estudo
// (algumas centenas de linhas por tabela), evita criar uma consulta
// especializada em cada repositório só pra alimentar o dashboard.
public class DashboardService(
    ITripRepository tripRepository,
    IVehicleRepository vehicleRepository,
    IDocumentRepository documentRepository,
    IOccurrenceRepository occurrenceRepository,
    IFuelingRepository fuelingRepository,
    ITripFinancialService financialService) : IDashboardService
{
    private const int RevenueTrendDays = 14;

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var trips = await tripRepository.GetAllAsync(cancellationToken);
        var vehicles = await vehicleRepository.GetAllAsync(cancellationToken);
        var documents = await documentRepository.GetAllAsync(cancellationToken);
        var occurrences = await occurrenceRepository.GetAllAsync(cancellationToken);
        var fuelings = await fuelingRepository.GetAllAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var today = now.Date;

        var completedTrips = trips.Where(t => t.Status == TripStatus.Concluida).ToList();
        var activeTrips = trips.Where(t => t.Status == TripStatus.EmAndamento).ToList();

        var deliveriesToday = completedTrips.Count(t => t.ActualArrivalAt?.Date == today);
        var delayedTrips = activeTrips.Count(t => t.EstimatedArrivalAt < now);

        var totalRevenue = completedTrips.Sum(t => t.FreightRevenue);

        decimal totalCost = 0;
        foreach (var trip in completedTrips)
        {
            var fuelingCost = await fuelingRepository.GetTotalCostByTripIdAsync(trip.Id, cancellationToken);
            totalCost += financialService.CalculateProfitability(trip, fuelingCost).TotalCost;
        }
        var aggregateMargin = totalRevenue - totalCost;
        var aggregateMarginPercentage = totalRevenue == 0 ? (decimal?)null : Math.Round(aggregateMargin / totalRevenue * 100, 2);

        var totalDistance = completedTrips.Sum(t => t.ActualDistanceKm ?? 0);
        var averageCostPerKm = totalDistance == 0 ? (decimal?)null : Math.Round(totalCost / totalDistance, 2);

        var unavailableVehicles = vehicles.Count(v => v.Status is VehicleStatus.EmManutencao or VehicleStatus.Indisponivel);

        var documentsNearExpiry = documents.Count(d => d.Status is DocumentStatus.ProximoVencimento or DocumentStatus.Vencido);
        var expiringDocuments = documents
            .Where(d => d.Status is DocumentStatus.ProximoVencimento or DocumentStatus.Vencido)
            .OrderBy(d => d.ExpiryDate)
            .Take(5)
            .Select(d => new ExpiringDocumentSummaryDto(d.Id, d.OwnerType.ToString(), d.Type.ToString(), d.ExpiryDate.ToString("yyyy-MM-dd"), d.Status.ToString()))
            .ToList();

        var openOccurrences = occurrences.Count(o => o.Status is OccurrenceStatus.Aberta or OccurrenceStatus.EmAndamento);
        var openOccurrenceList = occurrences
            .Where(o => o.Status is OccurrenceStatus.Aberta or OccurrenceStatus.EmAndamento)
            .OrderByDescending(o => o.OccurredAt)
            .Take(5)
            .Select(o => new OpenOccurrenceSummaryDto(o.Id, o.Vehicle?.PlateNumber ?? string.Empty, o.Type.ToString(), o.OccurredAt.ToString("O"), o.Status.ToString()))
            .ToList();

        var fleetEfficiency = CalculateFleetAverageFuelEfficiency(fuelings);

        var revenueByDay = BuildRevenueTrend(completedTrips, today);

        return new DashboardDto(
            activeTrips.Count,
            deliveriesToday,
            totalRevenue,
            totalRevenue, // faturamento = receita das viagens concluídas (o que virou/vira AccountReceivable)
            aggregateMargin,
            aggregateMarginPercentage,
            delayedTrips,
            unavailableVehicles,
            documentsNearExpiry,
            openOccurrences,
            fleetEfficiency,
            averageCostPerKm,
            revenueByDay,
            expiringDocuments,
            openOccurrenceList);
    }

    private static decimal? CalculateFleetAverageFuelEfficiency(List<Domain.Entities.Fuelings.Fueling> fuelings)
    {
        var ratios = new List<decimal>();

        foreach (var group in fuelings.GroupBy(f => f.VehicleId))
        {
            var ordered = group.OrderBy(f => f.FuelingDate).ThenBy(f => f.OdometerReading).ToList();
            for (var i = 1; i < ordered.Count; i++)
            {
                var distance = ordered[i].OdometerReading - ordered[i - 1].OdometerReading;
                if (distance <= 0 || ordered[i].LiterQuantity <= 0)
                    continue;
                ratios.Add(distance / ordered[i].LiterQuantity);
            }
        }

        return ratios.Count == 0 ? null : Math.Round(ratios.Average(), 2);
    }

    private static List<DailyRevenueDto> BuildRevenueTrend(List<Trip> completedTrips, DateTime today)
    {
        var byDay = completedTrips
            .Where(t => t.ActualArrivalAt is not null)
            .GroupBy(t => t.ActualArrivalAt!.Value.Date)
            .ToDictionary(g => g.Key, g => (Revenue: g.Sum(t => t.FreightRevenue), Deliveries: g.Count()));

        var trend = new List<DailyRevenueDto>();
        for (var i = RevenueTrendDays - 1; i >= 0; i--)
        {
            var day = today.AddDays(-i);
            var (revenue, deliveries) = byDay.TryGetValue(day, out var value) ? value : (0m, 0);
            trend.Add(new DailyRevenueDto(day.ToString("yyyy-MM-dd"), revenue, deliveries));
        }

        return trend;
    }
}
