namespace CargoFlow.Application.Finance;

public record FinanceSummaryDto(
    decimal TotalReceived,
    decimal TotalReceivablePending,
    decimal TotalReceivableOverdue,
    decimal TotalPaid,
    decimal TotalPayablePending,
    decimal TotalPayableOverdue,
    decimal NetMargin);

public interface IFinanceSummaryService
{
    Task<FinanceSummaryDto> GetSummaryAsync(CancellationToken cancellationToken);
}
