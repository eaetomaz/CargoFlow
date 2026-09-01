using MediatR;

namespace CargoFlow.Domain.Common;

// Raiz de agregado que acumula domain events (o vocabulário da seção 9 do
// documento de contexto -- TripStarted, DeliveryCompleted etc.) até serem
// publicados via MediatR pela Application, logo após um SaveChanges bem-
// sucedido. Sem broker real: é pub/sub em processo (ver plano de simulação).
public abstract class AggregateRoot : Entity
{
    private readonly List<INotification> _domainEvents = [];

    public IReadOnlyList<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(INotification domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public IReadOnlyList<INotification> PopDomainEvents()
    {
        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events;
    }
}
