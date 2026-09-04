using CargoFlow.Application.Companies;
using CargoFlow.Application.Freight;
using CargoFlow.Domain.Entities.Freight;
using CargoFlow.Domain.Entities.TransportOrders;
using System.Globalization;

namespace CargoFlow.Application.TransportOrders;

public class TransportOrderService(
    ITransportOrderRepository repository,
    ICompanyRepository companyRepository,
    IFreightQuoteRepository freightQuoteRepository) : ITransportOrderService
{
    public async Task<List<TransportOrderDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var orders = await repository.GetAllAsync(cancellationToken);
        return orders.Select(ToDto).ToList();
    }

    public async Task<TransportOrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : ToDto(order);
    }

    public async Task<TransportOrderDto> CreateAsync(CreateTransportOrderRequest request, CancellationToken cancellationToken)
    {
        var customer = await companyRepository.GetByIdAsync(request.CustomerCompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        var order = new TransportOrder
        {
            CustomerCompanyId = customer.Id,
            ShipperCompanyId = request.ShipperCompanyId,
            ConsigneeCompanyId = request.ConsigneeCompanyId,
            OriginCity = request.OriginCity,
            OriginState = request.OriginState,
            DestinationCity = request.DestinationCity,
            DestinationState = request.DestinationState,
            CargoDescription = request.CargoDescription,
            CargoWeightKg = request.CargoWeightKg,
            CargoValue = request.CargoValue,
            FreightValue = request.FreightValue,
            RequestedPickupDate = DateTime.Parse(request.RequestedPickupDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
            RequestedDeliveryDate = DateTime.Parse(request.RequestedDeliveryDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
        };

        await repository.AddAsync(order, cancellationToken);
        order.CustomerCompany = customer;
        return ToDto(order);
    }

    // "Uma cotação aprovada pode gerar uma Ordem de Transporte" (documento
    // de contexto, seção 5.6) -- ação manual e explícita, disparada de um
    // botão na cotação aprovada, não um efeito automático da aprovação.
    public async Task<TransportOrderDto> CreateFromFreightQuoteAsync(Guid freightQuoteId, string cargoDescription, string requestedPickupDate, string requestedDeliveryDate, CancellationToken cancellationToken)
    {
        var quote = await freightQuoteRepository.GetByIdAsync(freightQuoteId, cancellationToken)
            ?? throw new InvalidOperationException("Cotação não encontrada.");

        if (quote.Status != FreightQuoteStatus.Approved)
            throw new InvalidOperationException($"Só é possível gerar OT de uma cotação \"Approved\" -- esta está \"{quote.Status}\".");

        var order = new TransportOrder
        {
            FreightQuoteId = quote.Id,
            CustomerCompanyId = quote.CustomerCompanyId,
            OriginCity = quote.OriginCity,
            OriginState = quote.OriginState,
            DestinationCity = quote.DestinationCity,
            DestinationState = quote.DestinationState,
            CargoDescription = cargoDescription,
            CargoWeightKg = quote.CargoWeightKg,
            CargoValue = quote.CargoValue,
            FreightValue = quote.TotalValue,
            RequestedPickupDate = DateTime.Parse(requestedPickupDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
            RequestedDeliveryDate = DateTime.Parse(requestedDeliveryDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
        };

        await repository.AddAsync(order, cancellationToken);
        order.CustomerCompany = quote.CustomerCompany;
        return ToDto(order);
    }

    public async Task<TransportOrderDto> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Ordem de transporte não encontrada.");

        order.Cancel();
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    private static TransportOrderDto ToDto(TransportOrder o) => new(
        o.Id,
        o.FreightQuoteId,
        o.CustomerCompanyId,
        o.CustomerCompany?.Name ?? string.Empty,
        o.ShipperCompanyId,
        o.ConsigneeCompanyId,
        o.OriginCity,
        o.OriginState,
        o.DestinationCity,
        o.DestinationState,
        o.CargoDescription,
        o.CargoWeightKg,
        o.CargoValue,
        o.FreightValue,
        o.Status.ToString(),
        o.RequestedPickupDate.ToString("O"),
        o.RequestedDeliveryDate.ToString("O"));
}
