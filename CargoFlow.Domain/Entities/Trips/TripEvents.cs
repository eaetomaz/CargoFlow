using MediatR;

namespace CargoFlow.Domain.Entities.Trips;

// Vocabulário de eventos (seção 9 do documento de contexto), na parte que
// diz respeito ao ciclo de vida da viagem. Publicados via MediatR em
// processo (sem broker real) pela Application, logo após o SaveChanges que
// persistiu a mudança -- ver TripService/TripSchedulingService.
public record TripScheduledEvent(Guid TripId, Guid TransportOrderId, Guid VehicleId, Guid DriverId) : INotification;

public record TripStartedEvent(Guid TripId, Guid VehicleId, Guid DriverId) : INotification;

public record DeliveryCompletedEvent(Guid TripId, Guid TransportOrderId, decimal FreightRevenue) : INotification;

public record TripCancelledEvent(Guid TripId, Guid TransportOrderId) : INotification;
