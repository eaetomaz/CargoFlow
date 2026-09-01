using CargoFlow.Domain.Common;
using CargoFlow.Domain.Entities.Drivers;
using CargoFlow.Domain.Entities.Fleet;

namespace CargoFlow.Domain.Entities.Trips;

public enum TripStatus
{
    Programada,
    EmAndamento,
    Concluida,
    Cancelada,
}

// Único agregado do domínio com modelo rico de verdade (guards + domain
// events) -- é o objeto central do sistema (documento de contexto, seção
// 5.8) e o que o simulador movimenta. Custo/margem/custo-por-km são
// calculados sob demanda (TripFinancialService), nunca armazenados aqui.
public class Trip : AggregateRoot, IAuditable
{
    public Guid TransportOrderId { get; set; }
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public Guid? TrailerVehicleId { get; set; }
    public Guid DriverId { get; set; }
    public Driver? Driver { get; set; }

    public string OriginCity { get; set; } = string.Empty;
    public string OriginState { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public string DestinationState { get; set; } = string.Empty;

    public DateTime ScheduledDepartureAt { get; set; }
    public DateTime? ActualDepartureAt { get; set; }
    public DateTime EstimatedArrivalAt { get; set; }
    public DateTime? ActualArrivalAt { get; set; }

    public decimal PlannedDistanceKm { get; set; }
    public decimal? ActualDistanceKm { get; set; }

    public TripStatus Status { get; private set; } = TripStatus.Programada;
    public decimal FreightRevenue { get; set; }

    public List<TripExpense> Expenses { get; set; } = [];
    public List<TripEvent> Events { get; set; } = [];

    public static Trip Schedule(
        Guid transportOrderId, Guid vehicleId, Guid? trailerVehicleId, Guid driverId,
        string originCity, string originState, string destinationCity, string destinationState,
        DateTime scheduledDepartureAt, DateTime estimatedArrivalAt, decimal plannedDistanceKm, decimal freightRevenue,
        TripEventSource source = TripEventSource.Manual)
    {
        var trip = new Trip
        {
            TransportOrderId = transportOrderId,
            VehicleId = vehicleId,
            TrailerVehicleId = trailerVehicleId,
            DriverId = driverId,
            OriginCity = originCity,
            OriginState = originState,
            DestinationCity = destinationCity,
            DestinationState = destinationState,
            ScheduledDepartureAt = scheduledDepartureAt,
            EstimatedArrivalAt = estimatedArrivalAt,
            PlannedDistanceKm = plannedDistanceKm,
            FreightRevenue = freightRevenue,
            Status = TripStatus.Programada,
        };

        trip.AppendEvent(TripEventType.TripScheduled, scheduledDepartureAt, "Viagem programada.", source);
        trip.Raise(new TripScheduledEvent(trip.Id, transportOrderId, vehicleId, driverId));
        return trip;
    }

    public void Start(DateTime now, TripEventSource source = TripEventSource.Manual)
    {
        if (Status != TripStatus.Programada)
            throw new InvalidOperationException($"Só é possível iniciar uma viagem \"Programada\" -- esta está \"{Status}\".");

        Status = TripStatus.EmAndamento;
        ActualDepartureAt = now;
        UpdateTimestamp();

        AppendEvent(TripEventType.TripStarted, now, "Viagem iniciada.", source);
        Raise(new TripStartedEvent(Id, VehicleId, DriverId));
    }

    public void CompleteDelivery(DateTime now, decimal actualDistanceKm, TripEventSource source = TripEventSource.Manual)
    {
        if (Status != TripStatus.EmAndamento)
            throw new InvalidOperationException($"Só é possível concluir entrega de uma viagem \"EmAndamento\" -- esta está \"{Status}\".");

        Status = TripStatus.Concluida;
        ActualArrivalAt = now;
        ActualDistanceKm = actualDistanceKm;
        UpdateTimestamp();

        AppendEvent(TripEventType.DeliveryCompleted, now, "Entrega concluída.", source);
        Raise(new DeliveryCompletedEvent(Id, TransportOrderId, FreightRevenue));
    }

    public void Cancel(DateTime now, TripEventSource source = TripEventSource.Manual)
    {
        if (Status == TripStatus.Concluida)
            throw new InvalidOperationException("Viagem concluída não pode ser cancelada.");
        if (Status == TripStatus.Cancelada)
            return; // idempotente

        Status = TripStatus.Cancelada;
        UpdateTimestamp();

        AppendEvent(TripEventType.TripCancelled, now, "Viagem cancelada.", source);
        Raise(new TripCancelledEvent(Id, TransportOrderId));
    }

    // Regra 5 do documento de contexto: "uma viagem encerrada não pode
    // receber determinadas alterações" -- despesa é uma delas.
    public void AddExpense(TripExpense expense)
    {
        if (Status is TripStatus.Concluida or TripStatus.Cancelada)
            throw new InvalidOperationException($"Viagem \"{Status}\" não aceita novas despesas.");

        expense.TripId = Id;
        Expenses.Add(expense);
    }

    public void AppendEvent(TripEventType type, DateTime occurredAt, string description, TripEventSource source = TripEventSource.Manual)
    {
        Events.Add(new TripEvent
        {
            TripId = Id,
            Type = type,
            OccurredAt = occurredAt,
            Description = description,
            Source = source,
        });
    }
}
