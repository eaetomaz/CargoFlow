namespace CargoFlow.Application.Companies;

public record AddressDto(Guid Id, string Type, string Street, string Number, string? Complement, string District, string City, string State, string ZipCode, bool IsDefault);

public record ContactDto(Guid Id, string Name, string? Role, string? Email, string? Phone);

public record CompanyDto(
    Guid Id,
    string Name,
    string? TradeName,
    string Document,
    string DocumentType,
    string? StateRegistration,
    string? Email,
    string? Phone,
    string[] Roles,
    bool IsActive,
    List<AddressDto> Addresses,
    List<ContactDto> Contacts);

public record UpsertAddressRequest(string Type, string Street, string Number, string? Complement, string District, string City, string State, string ZipCode, bool IsDefault);

public record UpsertContactRequest(string Name, string? Role, string? Email, string? Phone);

public record UpsertCompanyRequest(
    string Name,
    string? TradeName,
    string Document,
    string DocumentType,
    string? StateRegistration,
    string? Email,
    string? Phone,
    string[] Roles,
    bool IsActive,
    List<UpsertAddressRequest> Addresses,
    List<UpsertContactRequest> Contacts);
