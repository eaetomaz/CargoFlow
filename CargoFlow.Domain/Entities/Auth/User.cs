using CargoFlow.Domain.Common;

namespace CargoFlow.Domain.Entities.Auth;

public enum UserRole
{
    Administrador,
    Operacional,
    Financeiro,
    Manutencao,
    Motorista,
}

public class User : Entity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    // Só preenchido quando Role == Motorista -- liga o login ao cadastro
    // do motorista (histórico de viagens, disponibilidade etc).
    public Guid? DriverId { get; set; }

    public bool IsActive { get; set; } = true;
}
