namespace CargoFlow.Application.TransportOrders;

public interface ITransportOrderService
{
    Task<List<TransportOrderDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<TransportOrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TransportOrderDto> CreateAsync(CreateTransportOrderRequest request, CancellationToken cancellationToken);
    Task<TransportOrderDto> CreateFromFreightQuoteAsync(Guid freightQuoteId, string cargoDescription, string requestedPickupDate, string requestedDeliveryDate, CancellationToken cancellationToken);
    Task<TransportOrderDto> CancelAsync(Guid id, CancellationToken cancellationToken);
}
