using CargoFlow.Application.Drivers;
using CargoFlow.Application.Fleet;
using CargoFlow.Application.Fuelings;
using CargoFlow.Application.TransportOrders;
using CargoFlow.Domain.Entities.Drivers;
using CargoFlow.Domain.Entities.Fleet;
using CargoFlow.Domain.Entities.Trips;
using MediatR;

namespace CargoFlow.Application.Trips;

public class TripService(
    ITripRepository tripRepository,
    ITransportOrderRepository transportOrderRepository,
    IVehicleRepository vehicleRepository,
    IDriverRepository driverRepository,
    IFuelingRepository fuelingRepository,
    ITripFinancialService financialService,
    IMediator mediator) : ITripService
{
    public async Task<List<TripDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var trips = await tripRepository.GetAllAsync(cancellationToken);
        return trips.Select(t => TripMapper.ToDto(t, t.Vehicle?.PlateNumber ?? string.Empty, t.Driver?.Name ?? string.Empty)).ToList();
    }

    public async Task<TripDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var trip = await tripRepository.GetByIdAsync(id, cancellationToken);
        return trip is null ? null : TripMapper.ToDto(trip, trip.Vehicle?.PlateNumber ?? string.Empty, trip.Driver?.Name ?? string.Empty);
    }

    public async Task<TripDto> StartAsync(Guid id, CancellationToken cancellationToken)
    {
        var trip = await GetTripOrThrowAsync(id, cancellationToken);
        var order = await transportOrderRepository.GetByIdAsync(trip.TransportOrderId, cancellationToken)
            ?? throw new InvalidOperationException("Ordem de transporte vinculada não encontrada.");

        trip.Start(DateTime.UtcNow);
        order.MarkInTransit();

        await tripRepository.SaveChangesAsync(cancellationToken);
        await PublishEventsAsync(trip, cancellationToken);

        return ToDto(trip);
    }

    public async Task<TripDto> AddExpenseAsync(Guid id, AddTripExpenseRequest request, CancellationToken cancellationToken)
    {
        var trip = await GetTripOrThrowAsync(id, cancellationToken);

        trip.AddExpense(new TripExpense
        {
            Type = Enum.Parse<TripExpenseType>(request.Type),
            Description = request.Description,
            Value = request.Value,
            ExpenseDate = DateOnly.Parse(request.ExpenseDate),
        });

        await tripRepository.SaveChangesAsync(cancellationToken);
        return ToDto(trip);
    }

    public async Task<TripDto> CompleteDeliveryAsync(Guid id, CompleteTripDeliveryRequest request, CancellationToken cancellationToken)
    {
        var trip = await GetTripOrThrowAsync(id, cancellationToken);
        var order = await transportOrderRepository.GetByIdAsync(trip.TransportOrderId, cancellationToken)
            ?? throw new InvalidOperationException("Ordem de transporte vinculada não encontrada.");

        trip.CompleteDelivery(DateTime.UtcNow, request.ActualDistanceKm);
        order.MarkDelivered();

        await ReleaseResourcesAsync(trip, cancellationToken);

        await tripRepository.SaveChangesAsync(cancellationToken);
        await PublishEventsAsync(trip, cancellationToken);

        return ToDto(trip);
    }

    public async Task<TripDto> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var trip = await GetTripOrThrowAsync(id, cancellationToken);

        trip.Cancel(DateTime.UtcNow);
        await ReleaseResourcesAsync(trip, cancellationToken);

        var order = await transportOrderRepository.GetByIdAsync(trip.TransportOrderId, cancellationToken);
        order?.Cancel();

        await tripRepository.SaveChangesAsync(cancellationToken);
        await PublishEventsAsync(trip, cancellationToken);

        return ToDto(trip);
    }

    public async Task<TripProfitabilityDto> GetProfitabilityAsync(Guid id, CancellationToken cancellationToken)
    {
        var trip = await GetTripOrThrowAsync(id, cancellationToken);
        var fuelingCost = await fuelingRepository.GetTotalCostByTripIdAsync(trip.Id, cancellationToken);
        var p = financialService.CalculateProfitability(trip, fuelingCost);
        return new TripProfitabilityDto(p.Revenue, p.TotalCost, p.Margin, p.MarginPercentage, p.CostPerKm);
    }

    private async Task<Trip> GetTripOrThrowAsync(Guid id, CancellationToken cancellationToken) =>
        await tripRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Viagem não encontrada.");

    // Libera veículo/motorista de volta pra "Disponível" -- espelha a
    // reserva feita em TripSchedulingService.ScheduleTripAsync.
    private async Task ReleaseResourcesAsync(Trip trip, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(trip.VehicleId, cancellationToken);
        if (vehicle is not null)
            vehicle.Status = VehicleStatus.Disponivel;

        var driver = await driverRepository.GetByIdAsync(trip.DriverId, cancellationToken);
        if (driver is not null)
            driver.AvailabilityStatus = DriverAvailabilityStatus.Disponivel;
    }

    private async Task PublishEventsAsync(Trip trip, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in trip.PopDomainEvents())
            await mediator.Publish(domainEvent, cancellationToken);
    }

    private static TripDto ToDto(Trip trip) => TripMapper.ToDto(trip, trip.Vehicle?.PlateNumber ?? string.Empty, trip.Driver?.Name ?? string.Empty);
}
