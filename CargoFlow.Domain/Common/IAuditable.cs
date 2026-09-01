namespace CargoFlow.Domain.Common;

// Marcador -- entidades que implementam essa interface têm Create/Update/Delete
// gravados em AuditLog pelo interceptor de SaveChanges do CargoFlowDbContext.
// Regra 10 do documento de contexto: "operações financeiras críticas devem
// ser auditáveis".
public interface IAuditable;
