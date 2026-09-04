using CargoFlow.Domain.Entities.Documents;
using Microsoft.Extensions.Configuration;

namespace CargoFlow.Application.Documents;

public class DocumentService(IDocumentRepository repository, IConfiguration configuration) : IDocumentService
{
    private int ExpiringSoonThresholdDays => configuration.GetValue("Documents:ExpiringSoonThresholdDays", 30);

    public async Task<List<DocumentDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var documents = await repository.GetAllAsync(cancellationToken);
        return documents.Select(ToDto).ToList();
    }

    public async Task<DocumentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(id, cancellationToken);
        return document is null ? null : ToDto(document);
    }

    public async Task<List<DocumentDto>> GetExpiringAsync(CancellationToken cancellationToken)
    {
        var documents = await repository.GetExpiringAsync(cancellationToken);
        return documents.Select(ToDto).ToList();
    }

    public async Task<DocumentDto> CreateAsync(UpsertDocumentRequest request, CancellationToken cancellationToken)
    {
        var document = new Document();
        Apply(document, request);

        await repository.AddAsync(document, cancellationToken);
        return ToDto(document);
    }

    public async Task<DocumentDto> UpdateAsync(Guid id, UpsertDocumentRequest request, CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Documento não encontrado.");

        Apply(document, request);
        document.UpdateTimestamp();

        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(document);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(id, cancellationToken);
        if (document is null)
            return; // idempotente

        await repository.DeleteAsync(document, cancellationToken);
    }

    private void Apply(Document document, UpsertDocumentRequest request)
    {
        document.OwnerType = Enum.Parse<DocumentOwnerType>(request.OwnerType);
        document.OwnerId = request.OwnerId;
        document.Type = Enum.Parse<DocumentType>(request.Type);
        document.Number = request.Number.Trim();
        document.IssueDate = request.IssueDate;
        document.ExpiryDate = request.ExpiryDate;
        document.AttachmentUrl = string.IsNullOrWhiteSpace(request.AttachmentUrl) ? null : request.AttachmentUrl.Trim();

        document.RecomputeStatus(DateOnly.FromDateTime(DateTime.UtcNow), ExpiringSoonThresholdDays);
    }

    private static DocumentDto ToDto(Document d) => new(
        d.Id,
        d.OwnerType.ToString(),
        d.OwnerId,
        d.Type.ToString(),
        d.Number,
        d.IssueDate,
        d.ExpiryDate,
        d.Status.ToString(),
        d.AttachmentUrl);
}
