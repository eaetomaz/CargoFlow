using System.Globalization;
using CargoFlow.Application.Documents;
using CargoFlow.Application.Drivers;
using CargoFlow.Application.Fleet;
using CargoFlow.Application.TransportOrders;
using CargoFlow.Domain.Entities.Documents;
using CargoFlow.Domain.Entities.Drivers;
using CargoFlow.Domain.Entities.Fleet;
using CargoFlow.Domain.Entities.Trips;
using MediatR;

namespace CargoFlow.Application.Trips;

// Orquestra a criação de uma viagem a partir de uma OT: valida as 3 regras
// de elegibilidade (motorista, veículo, compatibilidade) + documentos
// vencidos, e só então persiste. Separado de TripService (que cuida do
// ciclo de vida depois de já agendada) porque tem uma responsabilidade
// bem diferente -- decisão, não execução.
public class TripSchedulingService(
    ITripRepository tripRepository,
    ITransportOrderRepository transportOrderRepository,
    IVehicleRepository vehicleRepository,
    IDriverRepository driverRepository,
    IDocumentComplianceService complianceService,
    ITripSchedulingRules rules,
    IMediator mediator) : ITripSchedulingService
{
    public async Task<TripDto> ScheduleTripAsync(ScheduleTripRequest request, CancellationToken cancellationToken)
    {
        var order = await transportOrderRepository.GetByIdAsync(request.TransportOrderId, cancellationToken)
            ?? throw new InvalidOperationException("Ordem de transporte não encontrada.");

        var vehicle = await vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Veículo não encontrado.");

        var driver = await driverRepository.GetByIdAsync(request.DriverId, cancellationToken)
            ?? throw new InvalidOperationException("Motorista não encontrado.");

        var scheduledDepartureAt = DateTime.Parse(request.ScheduledDepartureAt, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        var estimatedArrivalAt = DateTime.Parse(request.EstimatedArrivalAt, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

        var vehicleHasBlockingDocs = await complianceService.HasBlockingExpiredDocumentsAsync(DocumentOwnerType.Vehicle, vehicle.Id, cancellationToken);
        var driverHasBlockingDocs = await complianceService.HasBlockingExpiredDocumentsAsync(DocumentOwnerType.Driver, driver.Id, cancellationToken);

        rules.ValidateVehicleEligibility(vehicle, vehicleHasBlockingDocs);
        rules.ValidateVehicleCompatibility(vehicle, order);
        rules.ValidateDriverEligibility(driver, scheduledDepartureAt, driverHasBlockingDocs);

        if (request.TrailerVehicleId is { } trailerId)
        {
            var trailer = await vehicleRepository.GetByIdAsync(trailerId, cancellationToken)
                ?? throw new InvalidOperationException("Reboque não encontrado.");
            if (trailer.Status != VehicleStatus.Disponivel)
                throw new InvalidOperationException($"Reboque {trailer.PlateNumber} não está disponível (status atual: {trailer.Status}).");
        }

        var trip = Trip.Schedule(
            order.Id, vehicle.Id, request.TrailerVehicleId, driver.Id,
            order.OriginCity, order.OriginState, order.DestinationCity, order.DestinationState,
            scheduledDepartureAt, estimatedArrivalAt, request.PlannedDistanceKm, order.FreightValue);

        order.MarkScheduled();

        // Reserva o veículo/motorista já na programação (não só quando a
        // viagem sai de fato) -- evita escalar o mesmo recurso em duas
        // viagens sobrepostas antes de qualquer uma delas partir.
        vehicle.Status = VehicleStatus.EmViagem;
        driver.AvailabilityStatus = DriverAvailabilityStatus.EmViagem;

        await tripRepository.AddAsync(trip, cancellationToken);

        foreach (var domainEvent in trip.PopDomainEvents())
            await mediator.Publish(domainEvent, cancellationToken);

        return TripMapper.ToDto(trip, vehicle.PlateNumber, driver.Name);
    }
}
