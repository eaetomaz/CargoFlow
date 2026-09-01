using CargoFlow.Domain.Common;

namespace CargoFlow.Domain.Entities.Trips;

public enum TripEventType
{
    TripScheduled,
    TripStarted,
    VehicleStopped,
    VehicleResumed,
    OccurrenceCreated,
    FuelingRegistered,
    DeliveryCompleted,
    BillingGenerated,
    TripCancelled,
}

public enum TripEventSource
{
    Manual,
    Simulator,
    System,
}

// Log append-only da timeline da viagem -- base da UI de linha do tempo e
// do drill-down do dashboard. Gravado diretamente pelos métodos do próprio
// Trip (não via handler de evento) porque é sempre a mesma transação/
// agregado; os domain events do MediatR (TripEvents.cs) são só pra efeitos
// que atravessam OUTROS agregados (ex: gerar faturamento).
public class TripEvent : Entity
{
    public Guid TripId { get; set; }
    public Trip? Trip { get; set; }

    public TripEventType Type { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public TripEventSource Source { get; set; } = TripEventSource.Manual;
}
