using CargoFlow.Domain.Common;
using CargoFlow.Domain.Entities.Companies;

namespace CargoFlow.Domain.Entities.Finance;

public enum AccountPayableCategory
{
    Combustivel,
    Manutencao,
    Pedagio,
    Fornecedor,
    Despesa,
}

public enum AccountPayableStatus
{
    Pendente,
    Pago,
    Vencido,
    Cancelado,
}

public class AccountPayable : Entity, IAuditable
{
    public AccountPayableCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;

    public Guid? SupplierCompanyId { get; set; }
    public Company? SupplierCompany { get; set; }

    // Link polimórfico opcional pra origem do custo (Fueling/MaintenanceOrder/
    // TripExpense) -- guardado só como referência informativa, nada no
    // sistema hoje cria um AccountPayable automaticamente a partir desses
    // módulos (lançamento é manual, como na maioria dos TMS reais: o
    // financeiro lança a partir da nota/fatura recebida).
    public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }

    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly? PaidDate { get; set; }
    public AccountPayableStatus Status { get; set; } = AccountPayableStatus.Pendente;

    public void Pay(DateOnly paidDate)
    {
        if (Status is AccountPayableStatus.Pago or AccountPayableStatus.Cancelado)
            throw new InvalidOperationException($"Conta a pagar \"{Status}\" não pode ser paga novamente.");

        Status = AccountPayableStatus.Pago;
        PaidDate = paidDate;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        if (Status == AccountPayableStatus.Pago)
            throw new InvalidOperationException("Conta a pagar já paga não pode ser cancelada.");

        Status = AccountPayableStatus.Cancelado;
        UpdateTimestamp();
    }

    public void RecomputeOverdue(DateOnly today)
    {
        if (Status == AccountPayableStatus.Pendente && DueDate < today)
            Status = AccountPayableStatus.Vencido;
    }
}
