using CargoFlow.Domain.Common;

namespace CargoFlow.Domain.Entities.Documents;

public enum DocumentOwnerType
{
    Vehicle,
    Driver,
    Company,
}

public enum DocumentType
{
    Cnh,
    Crlv,
    ApoliceSeguro,
    Licenciamento,
    Antt,
    ContratoTransporte,
    Outro,
}

public enum DocumentStatus
{
    Valido,
    ProximoVencimento,
    Vencido,
}

// Polimórfico via OwnerType/OwnerId (cobre Vehicle/Driver/Company) -- evita
// 3 tabelas quase-idênticas só pra guardar validade de documento.
public class Document : Entity, IAuditable
{
    public DocumentOwnerType OwnerType { get; set; }
    public Guid OwnerId { get; set; }

    public DocumentType Type { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Valido;
    public string? AttachmentUrl { get; set; }

    // Documentos críticos (CNH, CRLV, ApoliceSeguro) bloqueiam operação
    // quando vencidos -- ver DocumentComplianceService. Os demais (Outro,
    // ContratoTransporte...) só geram alerta.
    public static readonly DocumentType[] BlockingTypes = [DocumentType.Cnh, DocumentType.Crlv, DocumentType.ApoliceSeguro];

    public void RecomputeStatus(DateOnly today, int expiringSoonThresholdDays)
    {
        if (ExpiryDate < today)
        {
            Status = DocumentStatus.Vencido;
        }
        else if (ExpiryDate <= today.AddDays(expiringSoonThresholdDays))
        {
            Status = DocumentStatus.ProximoVencimento;
        }
        else
        {
            Status = DocumentStatus.Valido;
        }
    }
}
