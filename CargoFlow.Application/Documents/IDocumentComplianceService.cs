using CargoFlow.Domain.Entities.Documents;

namespace CargoFlow.Application.Documents;

public interface IDocumentComplianceService
{
    Task<bool> HasBlockingExpiredDocumentsAsync(DocumentOwnerType ownerType, Guid ownerId, CancellationToken cancellationToken);
}
