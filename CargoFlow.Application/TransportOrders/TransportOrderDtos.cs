namespace CargoFlow.Application.TransportOrders;

public record TransportOrderDto(
    Guid Id,
    Guid? FreightQuoteId,
    Guid CustomerCompanyId,
    string CustomerCompanyName,
    Guid? ShipperCompanyId,
    Guid? ConsigneeCompanyId,
    string OriginCity,
    string OriginState,
    string DestinationCity,
    string DestinationState,
    string CargoDescription,
    decimal CargoWeightKg,
    decimal CargoValue,
    decimal FreightValue,
    string Status,
    string RequestedPickupDate,
    string RequestedDeliveryDate);

public record CreateTransportOrderRequest(
    Guid CustomerCompanyId,
    Guid? ShipperCompanyId,
    Guid? ConsigneeCompanyId,
    string OriginCity,
    string OriginState,
    string DestinationCity,
    string DestinationState,
    string CargoDescription,
    decimal CargoWeightKg,
    decimal CargoValue,
    decimal FreightValue,
    string RequestedPickupDate,
    string RequestedDeliveryDate);
