namespace CargoFlow.Application.Drivers;

public record DriverDto(
    Guid Id,
    string Name,
    string Cpf,
    DateOnly BirthDate,
    string? Phone,
    string? Email,
    string CnhNumber,
    string CnhCategory,
    DateOnly CnhExpiryDate,
    DateOnly HireDate,
    string EmploymentType,
    string AvailabilityStatus);

public record UpsertDriverRequest(
    string Name,
    string Cpf,
    DateOnly BirthDate,
    string? Phone,
    string? Email,
    string CnhNumber,
    string CnhCategory,
    DateOnly CnhExpiryDate,
    DateOnly HireDate,
    string EmploymentType,
    string AvailabilityStatus);
