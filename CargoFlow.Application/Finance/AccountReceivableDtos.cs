namespace CargoFlow.Application.Finance;

public record AccountReceivableDto(
    Guid Id,
    Guid CustomerCompanyId,
    string CustomerCompanyName,
    Guid? TripId,
    Guid? TransportOrderId,
    decimal Amount,
    string DueDate,
    string? ReceivedDate,
    string Status);

public record CreateAccountReceivableRequest(
    Guid CustomerCompanyId,
    Guid? TripId,
    Guid? TransportOrderId,
    decimal Amount,
    string DueDate);
