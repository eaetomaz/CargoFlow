using CargoFlow.Domain.Entities.Audit;

namespace CargoFlow.Application.Audit;

public interface IAuditLogRepository
{
    Task<List<AuditLog>> GetAsync(string? entityName, Guid? entityId, CancellationToken cancellationToken);
}
