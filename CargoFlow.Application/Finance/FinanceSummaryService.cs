using CargoFlow.Domain.Entities.Finance;

namespace CargoFlow.Application.Finance;

public class FinanceSummaryService(IAccountPayableRepository payableRepository, IAccountReceivableRepository receivableRepository) : IFinanceSummaryService
{
    public async Task<FinanceSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var payables = await payableRepository.GetAllAsync(cancellationToken);
        var receivables = await receivableRepository.GetAllAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var p in payables) p.RecomputeOverdue(today);
        foreach (var r in receivables) r.RecomputeOverdue(today);

        var totalReceived = receivables.Where(r => r.Status == AccountReceivableStatus.Recebido).Sum(r => r.Amount);
        var totalReceivablePending = receivables.Where(r => r.Status == AccountReceivableStatus.Pendente).Sum(r => r.Amount);
        var totalReceivableOverdue = receivables.Where(r => r.Status == AccountReceivableStatus.Vencido).Sum(r => r.Amount);

        var totalPaid = payables.Where(p => p.Status == AccountPayableStatus.Pago).Sum(p => p.Amount);
        var totalPayablePending = payables.Where(p => p.Status == AccountPayableStatus.Pendente).Sum(p => p.Amount);
        var totalPayableOverdue = payables.Where(p => p.Status == AccountPayableStatus.Vencido).Sum(p => p.Amount);

        return new FinanceSummaryDto(
            totalReceived,
            totalReceivablePending,
            totalReceivableOverdue,
            totalPaid,
            totalPayablePending,
            totalPayableOverdue,
            totalReceived - totalPaid);
    }
}
