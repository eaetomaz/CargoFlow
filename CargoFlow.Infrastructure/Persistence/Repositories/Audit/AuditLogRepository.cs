using CargoFlow.Application.Audit;
using CargoFlow.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence.Repositories.Audit;

public class AuditLogRepository(CargoFlowDbContext context) : IAuditLogRepository
{
    public Task<List<AuditLog>> GetAsync(string? entityName, Guid? entityId, CancellationToken cancellationToken)
    {
        var query = context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(a => a.EntityName == entityName);
        if (entityId is { } id)
            query = query.Where(a => a.EntityId == id);

        return query.OrderByDescending(a => a.ChangedAt).Take(500).ToListAsync(cancellationToken);
    }
}
