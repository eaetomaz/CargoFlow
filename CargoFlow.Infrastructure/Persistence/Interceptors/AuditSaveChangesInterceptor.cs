using System.Security.Claims;
using System.Text.Json;
using CargoFlow.Domain.Common;
using CargoFlow.Domain.Entities.Audit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CargoFlow.Infrastructure.Persistence.Interceptors;

// Regra 10 do documento de contexto: "operações financeiras críticas devem
// ser auditáveis" -- implementado de forma genérica pra toda entidade
// IAuditable (não só as financeiras), gravado na MESMA transação do
// SaveChanges que originou a mudança. Nunca chamado manualmente por um
// service -- registrado uma vez em AddInfrastructure.
public class AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AppendAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AppendAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void AppendAuditLogs(DbContext? context)
    {
        if (context is null)
            return;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditable && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (entries.Count == 0)
            return;

        var (userId, userName) = GetCurrentUser();

        foreach (var entry in entries)
        {
            var operation = entry.State switch
            {
                EntityState.Added => AuditOperation.Create,
                EntityState.Deleted => AuditOperation.Delete,
                _ => AuditOperation.Update,
            };

            context.Set<AuditLog>().Add(new AuditLog
            {
                UserId = userId,
                UserName = userName,
                EntityName = entry.Entity.GetType().Name,
                EntityId = entry.Property(nameof(Entity.Id)).CurrentValue is Guid id ? id : Guid.Empty,
                Operation = operation,
                OldValuesJson = operation == AuditOperation.Create ? null : Serialize(entry, useOriginalValues: true),
                NewValuesJson = operation == AuditOperation.Delete ? null : Serialize(entry, useOriginalValues: false),
            });
        }
    }

    private static string Serialize(EntityEntry entry, bool useOriginalValues)
    {
        var values = new Dictionary<string, object?>();
        foreach (var property in entry.Properties)
        {
            var value = useOriginalValues ? property.OriginalValue : property.CurrentValue;
            values[property.Metadata.Name] = value;
        }
        return JsonSerializer.Serialize(values);
    }

    private (Guid? userId, string userName) GetCurrentUser()
    {
        var user = httpContextAccessor.HttpContext?.User;
        var idClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user?.FindFirst("sub")?.Value;
        var nameClaim = user?.FindFirst(ClaimTypes.Name)?.Value;

        if (Guid.TryParse(idClaim, out var userId))
            return (userId, nameClaim ?? "desconhecido");

        // Sem HttpContext (seed no startup, ou chamada do simulador em
        // background) -- audita mesmo assim, só sem usuário atribuído.
        return (null, "sistema");
    }
}
