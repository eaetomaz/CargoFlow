using CargoFlow.Domain.Entities.Audit;

namespace CargoFlow.Application.Audit;

public class AuditLogService(IAuditLogRepository repository) : IAuditLogService
{
    public async Task<List<AuditLogDto>> GetAsync(string? entityName, Guid? entityId, CancellationToken cancellationToken)
    {
        var logs = await repository.GetAsync(entityName, entityId, cancellationToken);
        return logs.Select(ToDto).ToList();
    }

    private static AuditLogDto ToDto(AuditLog log) => new(
        log.Id,
        log.UserId,
        log.UserName,
        log.EntityName,
        log.EntityId,
        log.Operation.ToString(),
        log.OldValuesJson,
        log.NewValuesJson,
        log.ChangedAt.ToString("O"),
        log.Source.ToString());
}
