using CargoFlow.Domain.Entities.Documents;

namespace CargoFlow.Application.Documents;

public class DocumentComplianceService(IDocumentRepository repository) : IDocumentComplianceService
{
    public Task<bool> HasBlockingExpiredDocumentsAsync(DocumentOwnerType ownerType, Guid ownerId, CancellationToken cancellationToken) =>
        repository.HasBlockingExpiredAsync(ownerType, ownerId, cancellationToken);
}
