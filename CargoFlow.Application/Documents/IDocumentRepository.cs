using CargoFlow.Domain.Entities.Documents;

namespace CargoFlow.Application.Documents;

public interface IDocumentRepository
{
    Task<List<Document>> GetAllAsync(CancellationToken cancellationToken);
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Document>> GetExpiringAsync(CancellationToken cancellationToken);

    // Usado pelo TripSchedulingService: CNH/CRLV/Apólice vencidos bloqueiam
    // a programação (regra 4 do documento de contexto).
    Task<bool> HasBlockingExpiredAsync(DocumentOwnerType ownerType, Guid ownerId, CancellationToken cancellationToken);
    Task AddAsync(Document document, CancellationToken cancellationToken);
    Task DeleteAsync(Document document, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
