using CargoFlow.Domain.Common;
using CargoFlow.Domain.Entities.Companies;

namespace CargoFlow.Domain.Entities.Finance;

public enum AccountReceivableStatus
{
    Pendente,
    Recebido,
    Vencido,
    Cancelado,
}

// "Faturamento" (BillingGenerated, seção 9 do documento de contexto) é
// literalmente a criação de um AccountReceivable a partir de uma entrega
// concluída -- ver BillingGenerationHandler. Não existe entidade de Invoice
// separada, o próprio AccountReceivable já é o registro do faturamento.
public class AccountReceivable : Entity, IAuditable
{
    public Guid CustomerCompanyId { get; set; }
    public Company? CustomerCompany { get; set; }

    public Guid? TripId { get; set; }
    public Guid? TransportOrderId { get; set; }

    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly? ReceivedDate { get; set; }
    public AccountReceivableStatus Status { get; set; } = AccountReceivableStatus.Pendente;

    public void Receive(DateOnly receivedDate)
    {
        if (Status is AccountReceivableStatus.Recebido or AccountReceivableStatus.Cancelado)
            throw new InvalidOperationException($"Conta a receber \"{Status}\" não pode ser recebida novamente.");

        Status = AccountReceivableStatus.Recebido;
        ReceivedDate = receivedDate;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        if (Status == AccountReceivableStatus.Recebido)
            throw new InvalidOperationException("Conta a receber já recebida não pode ser cancelada.");

        Status = AccountReceivableStatus.Cancelado;
        UpdateTimestamp();
    }

    public void RecomputeOverdue(DateOnly today)
    {
        if (Status == AccountReceivableStatus.Pendente && DueDate < today)
            Status = AccountReceivableStatus.Vencido;
    }
}
