using CargoFlow.Application.TransportOrders;
using CargoFlow.Domain.Entities.Finance;
using CargoFlow.Domain.Entities.Trips;
using MediatR;

namespace CargoFlow.Application.Finance;

// Regra 6 do documento de contexto: "uma entrega concluída pode gerar
// faturamento". Reage a DeliveryCompletedEvent (publicado por TripService
// logo após CompleteDelivery) criando o AccountReceivable e movendo a OT
// pra Faturada. Regra 9 (idempotência): checa se já existe um recebível
// pra essa viagem antes de criar -- protege contra o mesmo evento sendo
// publicado mais de uma vez (retry, ou futuramente o simulador).
public class BillingGenerationHandler(
    IAccountReceivableRepository receivableRepository,
    ITransportOrderRepository transportOrderRepository) : INotificationHandler<DeliveryCompletedEvent>
{
    private const int PaymentTermDays = 30;

    public async Task Handle(DeliveryCompletedEvent notification, CancellationToken cancellationToken)
    {
        if (await receivableRepository.ExistsForTripAsync(notification.TripId, cancellationToken))
            return; // idempotente -- já faturado

        var order = await transportOrderRepository.GetByIdAsync(notification.TransportOrderId, cancellationToken);
        if (order is null)
            return;

        var receivable = new AccountReceivable
        {
            CustomerCompanyId = order.CustomerCompanyId,
            TripId = notification.TripId,
            TransportOrderId = notification.TransportOrderId,
            Amount = notification.FreightRevenue,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(PaymentTermDays)),
        };

        await receivableRepository.AddAsync(receivable, cancellationToken);

        order.MarkBilled();
        await transportOrderRepository.SaveChangesAsync(cancellationToken);
    }
}
