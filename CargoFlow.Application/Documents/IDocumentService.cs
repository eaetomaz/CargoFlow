namespace CargoFlow.Application.Documents;

public interface IDocumentService
{
    Task<List<DocumentDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<DocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<DocumentDto>> GetExpiringAsync(CancellationToken cancellationToken);
    Task<DocumentDto> CreateAsync(UpsertDocumentRequest request, CancellationToken cancellationToken);
    Task<DocumentDto> UpdateAsync(Guid id, UpsertDocumentRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
