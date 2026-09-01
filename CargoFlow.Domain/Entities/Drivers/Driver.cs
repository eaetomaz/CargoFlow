using CargoFlow.Domain.Common;

namespace CargoFlow.Domain.Entities.Drivers;

public enum CnhCategory
{
    A,
    B,
    C,
    D,
    E,
}

public enum EmploymentType
{
    Clt,
    Autonomo,
    Agregado,
}

public enum DriverAvailabilityStatus
{
    Disponivel,
    EmViagem,
    Ferias,
    Afastado,
    Inativo,
}

public class Driver : Entity, IAuditable
{
    public string Name { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string CnhNumber { get; set; } = string.Empty;
    public CnhCategory CnhCategory { get; set; }
    public DateOnly CnhExpiryDate { get; set; }
    public DateOnly HireDate { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public DriverAvailabilityStatus AvailabilityStatus { get; set; } = DriverAvailabilityStatus.Disponivel;
}
