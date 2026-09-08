namespace CargoFlow.Application.Dashboard;

public record DailyRevenueDto(string Date, decimal Revenue, int Deliveries);

public record ExpiringDocumentSummaryDto(Guid Id, string OwnerType, string Type, string ExpiryDate, string Status);

public record OpenOccurrenceSummaryDto(Guid Id, string VehiclePlate, string Type, string OccurredAt, string Status);

public record DashboardDto(
    int ActiveTripsCount,
    int DeliveriesTodayCount,
    decimal TotalRevenue,
    decimal TotalBilled,
    decimal AggregateMargin,
    decimal? AggregateMarginPercentage,
    int DelayedTripsCount,
    int UnavailableVehiclesCount,
    int DocumentsNearExpiryCount,
    int OpenOccurrencesCount,
    decimal? FleetAverageFuelEfficiencyKmL,
    decimal? AverageCostPerKm,
    List<DailyRevenueDto> RevenueByDay,
    List<ExpiringDocumentSummaryDto> ExpiringDocuments,
    List<OpenOccurrenceSummaryDto> OpenOccurrences);
