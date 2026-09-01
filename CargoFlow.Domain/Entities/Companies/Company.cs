using CargoFlow.Domain.Common;

namespace CargoFlow.Domain.Entities.Companies;

// Multi-papel via [Flags] -- uma empresa pode ser cliente E fornecedora ao
// mesmo tempo, por exemplo. Evita duplicar cadastro por papel.
[Flags]
public enum CompanyRoles
{
    None = 0,
    Cliente = 1,
    Embarcador = 2,
    Destinatario = 4,
    Fornecedor = 8,
    Parceiro = 16,
}

public enum CompanyDocumentType
{
    Cnpj,
    Cpf,
}

public class Company : Entity, IAuditable
{
    public string Name { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public string Document { get; set; } = string.Empty;
    public CompanyDocumentType DocumentType { get; set; }
    public string? StateRegistration { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public CompanyRoles Roles { get; set; }
    public bool IsActive { get; set; } = true;

    public List<Address> Addresses { get; set; } = [];
    public List<Contact> Contacts { get; set; } = [];
}
