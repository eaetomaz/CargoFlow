using CargoFlow.Domain.Common;

namespace CargoFlow.Domain.Entities.Companies;

public class Contact : Entity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
