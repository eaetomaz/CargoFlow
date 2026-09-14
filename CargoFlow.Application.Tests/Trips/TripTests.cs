using CargoFlow.Domain.Entities.Trips;

namespace CargoFlow.Application.Tests.Trips;

// Testa os guards do próprio agregado Trip (regra 5 do documento de
// contexto: "uma viagem encerrada não pode receber determinadas
// alterações"), sem passar por nenhum service/repositório.
public class TripTests
{
    private static Trip NewScheduledTrip() =>
        Trip.Schedule(
            Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(),
            "Origem", "OR", "Destino", "DE",
            DateTime.UtcNow, DateTime.UtcNow.AddDays(1), plannedDistanceKm: 100m, freightRevenue: 1000m);

    [Fact]
    public void AddExpense_Throws_WhenTripIsConcluida()
    {
        var trip = NewScheduledTrip();
        trip.Start(DateTime.UtcNow);
        trip.CompleteDelivery(DateTime.UtcNow, actualDistanceKm: 100m);

        Assert.Throws<InvalidOperationException>(() =>
            trip.AddExpense(new TripExpense { Type = TripExpenseType.Diversos, Value = 50m, ExpenseDate = DateOnly.FromDateTime(DateTime.UtcNow) }));
    }

    [Fact]
    public void AddExpense_Succeeds_WhileTripInProgress()
    {
        var trip = NewScheduledTrip();
        trip.Start(DateTime.UtcNow);

        trip.AddExpense(new TripExpense { Type = TripExpenseType.Pedagio, Value = 50m, ExpenseDate = DateOnly.FromDateTime(DateTime.UtcNow) });

        Assert.Single(trip.Expenses);
    }

    [Fact]
    public void Start_Throws_WhenTripAlreadyStarted()
    {
        var trip = NewScheduledTrip();
        trip.Start(DateTime.UtcNow);

        Assert.Throws<InvalidOperationException>(() => trip.Start(DateTime.UtcNow));
    }

    [Fact]
    public void CompleteDelivery_Throws_WhenTripNotYetStarted()
    {
        var trip = NewScheduledTrip();

        Assert.Throws<InvalidOperationException>(() => trip.CompleteDelivery(DateTime.UtcNow, actualDistanceKm: 100m));
    }

    [Fact]
    public void Cancel_Throws_WhenTripAlreadyConcluded()
    {
        var trip = NewScheduledTrip();
        trip.Start(DateTime.UtcNow);
        trip.CompleteDelivery(DateTime.UtcNow, actualDistanceKm: 100m);

        Assert.Throws<InvalidOperationException>(() => trip.Cancel(DateTime.UtcNow));
    }

    [Fact]
    public void Schedule_RaisesTripScheduledDomainEvent()
    {
        var trip = NewScheduledTrip();

        var events = trip.PopDomainEvents();

        Assert.Single(events);
        Assert.IsType<TripScheduledEvent>(events[0]);
    }

    [Fact]
    public void CompleteDelivery_RaisesDeliveryCompletedEvent_WithFreightRevenue()
    {
        var trip = NewScheduledTrip();
        trip.PopDomainEvents(); // descarta o TripScheduledEvent da criação
        trip.Start(DateTime.UtcNow);
        trip.PopDomainEvents(); // descarta o TripStartedEvent

        trip.CompleteDelivery(DateTime.UtcNow, actualDistanceKm: 100m);
        var events = trip.PopDomainEvents();

        var deliveryEvent = Assert.IsType<DeliveryCompletedEvent>(Assert.Single(events));
        Assert.Equal(1000m, deliveryEvent.FreightRevenue);
    }
}
