namespace CargoFlow.Application.Finance;

public record AccountPayableDto(
    Guid Id,
    string Category,
    string Description,
    Guid? SupplierCompanyId,
    string? SupplierCompanyName,
    string? SourceType,
    Guid? SourceId,
    decimal Amount,
    string DueDate,
    string? PaidDate,
    string Status);

public record CreateAccountPayableRequest(
    string Category,
    string Description,
    Guid? SupplierCompanyId,
    string? SourceType,
    Guid? SourceId,
    decimal Amount,
    string DueDate);
