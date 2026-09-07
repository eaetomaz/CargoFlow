namespace CargoFlow.Application.Trips;

public record TripEventDto(Guid Id, string Type, string OccurredAt, string Description, string Source);

public record TripExpenseDto(Guid Id, string Type, string? Description, decimal Value, string ExpenseDate);

public record TripDto(
    Guid Id,
    Guid TransportOrderId,
    Guid VehicleId,
    string VehiclePlate,
    Guid? TrailerVehicleId,
    Guid DriverId,
    string DriverName,
    string OriginCity,
    string OriginState,
    string DestinationCity,
    string DestinationState,
    string ScheduledDepartureAt,
    string? ActualDepartureAt,
    string EstimatedArrivalAt,
    string? ActualArrivalAt,
    decimal PlannedDistanceKm,
    decimal? ActualDistanceKm,
    string Status,
    decimal FreightRevenue,
    List<TripExpenseDto> Expenses,
    List<TripEventDto> Events);

public record ScheduleTripRequest(
    Guid TransportOrderId,
    Guid VehicleId,
    Guid? TrailerVehicleId,
    Guid DriverId,
    string ScheduledDepartureAt,
    string EstimatedArrivalAt,
    decimal PlannedDistanceKm);

public record AddTripExpenseRequest(string Type, string? Description, decimal Value, string ExpenseDate);

public record CompleteTripDeliveryRequest(decimal ActualDistanceKm);

public record TripProfitabilityDto(decimal Revenue, decimal TotalCost, decimal Margin, decimal? MarginPercentage, decimal? CostPerKm);
