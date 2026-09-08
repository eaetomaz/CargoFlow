namespace CargoFlow.Application.Audit;

public record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string UserName,
    string EntityName,
    Guid EntityId,
    string Operation,
    string? OldValuesJson,
    string? NewValuesJson,
    string ChangedAt,
    string Source);
