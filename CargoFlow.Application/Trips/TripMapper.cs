using CargoFlow.Domain.Entities.Trips;

namespace CargoFlow.Application.Trips;

// Compartilhado entre TripSchedulingService e TripService -- os dois
// precisam montar o mesmo TripDto (placa/nome já resolvidos, não só os
// Guids) a partir de um Trip carregado.
public static class TripMapper
{
    public static TripDto ToDto(Trip t, string vehiclePlate, string driverName) => new(
        t.Id,
        t.TransportOrderId,
        t.VehicleId,
        vehiclePlate,
        t.TrailerVehicleId,
        t.DriverId,
        driverName,
        t.OriginCity,
        t.OriginState,
        t.DestinationCity,
        t.DestinationState,
        t.ScheduledDepartureAt.ToString("O"),
        t.ActualDepartureAt?.ToString("O"),
        t.EstimatedArrivalAt.ToString("O"),
        t.ActualArrivalAt?.ToString("O"),
        t.PlannedDistanceKm,
        t.ActualDistanceKm,
        t.Status.ToString(),
        t.FreightRevenue,
        t.Expenses
            .OrderByDescending(e => e.ExpenseDate)
            .Select(e => new TripExpenseDto(e.Id, e.Type.ToString(), e.Description, e.Value, e.ExpenseDate.ToString("yyyy-MM-dd")))
            .ToList(),
        t.Events
            .OrderBy(e => e.OccurredAt)
            .Select(e => new TripEventDto(e.Id, e.Type.ToString(), e.OccurredAt.ToString("O"), e.Description, e.Source.ToString()))
            .ToList());
}
