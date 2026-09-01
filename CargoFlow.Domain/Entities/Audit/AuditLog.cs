using CargoFlow.Domain.Common;

namespace CargoFlow.Domain.Entities.Audit;

public enum AuditOperation
{
    Create,
    Update,
    Delete,
}

public enum AuditSource
{
    UserAction,
    Simulator,
    System,
}

// Gravado automaticamente pelo AuditSaveChangesInterceptor pra toda entidade
// que implementa IAuditable -- nunca criado manualmente por um service.
public class AuditLog : Entity
{
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public AuditOperation Operation { get; set; }

    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public AuditSource Source { get; set; } = AuditSource.UserAction;
}
