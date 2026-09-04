namespace CargoFlow.Application.Documents;

public record DocumentDto(
    Guid Id,
    string OwnerType,
    Guid OwnerId,
    string Type,
    string Number,
    DateOnly IssueDate,
    DateOnly ExpiryDate,
    string Status,
    string? AttachmentUrl);

// Status é calculado por RecomputeStatus a partir de ExpiryDate -- nunca
// aceito do cliente, por isso não aparece aqui.
public record UpsertDocumentRequest(
    string OwnerType,
    Guid OwnerId,
    string Type,
    string Number,
    DateOnly IssueDate,
    DateOnly ExpiryDate,
    string? AttachmentUrl);
