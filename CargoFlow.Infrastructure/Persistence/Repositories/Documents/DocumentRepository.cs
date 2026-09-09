using CargoFlow.Application.Documents;
using CargoFlow.Domain.Entities.Documents;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence.Repositories.Documents;

public class DocumentRepository(CargoFlowDbContext context) : IDocumentRepository
{
    public Task<List<Document>> GetAllAsync(CancellationToken cancellationToken) =>
        context.Documents.OrderBy(d => d.ExpiryDate).ToListAsync(cancellationToken);

    public Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Documents.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<List<Document>> GetExpiringAsync(CancellationToken cancellationToken) =>
        context.Documents
            .Where(d => d.Status == DocumentStatus.ProximoVencimento || d.Status == DocumentStatus.Vencido)
            .OrderBy(d => d.ExpiryDate)
            .ToListAsync(cancellationToken);

    public Task<bool> HasBlockingExpiredAsync(DocumentOwnerType ownerType, Guid ownerId, CancellationToken cancellationToken) =>
        context.Documents.AnyAsync(
            d => d.OwnerType == ownerType && d.OwnerId == ownerId
                && d.Status == DocumentStatus.Vencido && Document.BlockingTypes.Contains(d.Type),
            cancellationToken);

    public async Task AddAsync(Document document, CancellationToken cancellationToken)
    {
        context.Documents.Add(document);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Document document, CancellationToken cancellationToken)
    {
        context.Documents.Remove(document);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
