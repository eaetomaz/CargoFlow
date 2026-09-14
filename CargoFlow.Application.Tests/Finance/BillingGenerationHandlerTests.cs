using CargoFlow.Application.Finance;
using CargoFlow.Application.TransportOrders;
using CargoFlow.Domain.Entities.Finance;
using CargoFlow.Domain.Entities.TransportOrders;
using CargoFlow.Domain.Entities.Trips;

namespace CargoFlow.Application.Tests.Finance;

// Regra 9 do documento de contexto: "eventos duplicados não podem gerar
// efeitos duplicados". Usa fakes em memória (sem banco) pra provar que
// publicar o mesmo DeliveryCompletedEvent duas vezes só gera UM
// AccountReceivable.
public class BillingGenerationHandlerTests
{
    private class FakeReceivableRepository : IAccountReceivableRepository
    {
        public readonly List<AccountReceivable> Saved = [];

        public Task<List<AccountReceivable>> GetAllAsync(CancellationToken cancellationToken) => Task.FromResult(Saved.ToList());
        public Task<AccountReceivable?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Saved.FirstOrDefault(r => r.Id == id));
        public Task<bool> ExistsForTripAsync(Guid tripId, CancellationToken cancellationToken) => Task.FromResult(Saved.Any(r => r.TripId == tripId));
        public Task AddAsync(AccountReceivable receivable, CancellationToken cancellationToken) { Saved.Add(receivable); return Task.CompletedTask; }
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private class FakeTransportOrderRepository(TransportOrder order) : ITransportOrderRepository
    {
        public Task<List<TransportOrder>> GetAllAsync(CancellationToken cancellationToken) => Task.FromResult(new List<TransportOrder> { order });
        public Task<TransportOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == order.Id ? order : null);
        public Task AddAsync(TransportOrder o, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    [Fact]
    public async Task Handle_PublishedTwiceForSameTrip_OnlyCreatesOneReceivable()
    {
        var order = new TransportOrder { CustomerCompanyId = Guid.NewGuid() };
        order.MarkScheduled();
        order.MarkInTransit();
        order.MarkDelivered();

        var receivables = new FakeReceivableRepository();
        var orders = new FakeTransportOrderRepository(order);
        var handler = new BillingGenerationHandler(receivables, orders);

        var tripId = Guid.NewGuid();
        var domainEvent = new DeliveryCompletedEvent(tripId, order.Id, FreightRevenue: 5000m);

        await handler.Handle(domainEvent, CancellationToken.None);
        await handler.Handle(domainEvent, CancellationToken.None); // duplicado

        Assert.Single(receivables.Saved);
        Assert.Equal(5000m, receivables.Saved[0].Amount);
    }

    [Fact]
    public async Task Handle_MarksTransportOrderAsBilled()
    {
        var order = new TransportOrder { CustomerCompanyId = Guid.NewGuid() };
        order.MarkScheduled();
        order.MarkInTransit();
        order.MarkDelivered();

        var handler = new BillingGenerationHandler(new FakeReceivableRepository(), new FakeTransportOrderRepository(order));
        await handler.Handle(new DeliveryCompletedEvent(Guid.NewGuid(), order.Id, 1000m), CancellationToken.None);

        Assert.Equal(TransportOrderStatus.Faturada, order.Status);
    }
}
