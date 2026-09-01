using CargoFlow.Domain.Common;

namespace CargoFlow.Domain.Entities.Companies;

public enum AddressType
{
    Principal,
    Cobranca,
    Entrega,
}

public class Address : Entity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public AddressType Type { get; set; }
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Complement { get; set; }
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
