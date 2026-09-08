namespace CargoFlow.Application.Audit;

public interface IAuditLogService
{
    Task<List<AuditLogDto>> GetAsync(string? entityName, Guid? entityId, CancellationToken cancellationToken);
}
