using CargoFlow.Application.Companies;
using CargoFlow.Domain.Entities.Finance;

namespace CargoFlow.Application.Finance;

public class AccountReceivableService(IAccountReceivableRepository repository, ICompanyRepository companyRepository) : IAccountReceivableService
{
    public async Task<List<AccountReceivableDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var receivables = await repository.GetAllAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var changed = false;
        foreach (var r in receivables)
        {
            var before = r.Status;
            r.RecomputeOverdue(today);
            if (r.Status != before) changed = true;
        }
        if (changed) await repository.SaveChangesAsync(cancellationToken);

        return receivables.Select(ToDto).ToList();
    }

    public async Task<AccountReceivableDto> CreateAsync(CreateAccountReceivableRequest request, CancellationToken cancellationToken)
    {
        var customer = await companyRepository.GetByIdAsync(request.CustomerCompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        var receivable = new AccountReceivable
        {
            CustomerCompanyId = customer.Id,
            TripId = request.TripId,
            TransportOrderId = request.TransportOrderId,
            Amount = request.Amount,
            DueDate = DateOnly.Parse(request.DueDate),
        };

        await repository.AddAsync(receivable, cancellationToken);
        receivable.CustomerCompany = customer;
        return ToDto(receivable);
    }

    public async Task<AccountReceivableDto> ReceiveAsync(Guid id, CancellationToken cancellationToken)
    {
        var receivable = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Conta a receber não encontrada.");

        receivable.Receive(DateOnly.FromDateTime(DateTime.UtcNow));
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(receivable);
    }

    public async Task<AccountReceivableDto> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var receivable = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Conta a receber não encontrada.");

        receivable.Cancel();
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(receivable);
    }

    private static AccountReceivableDto ToDto(AccountReceivable r) => new(
        r.Id,
        r.CustomerCompanyId,
        r.CustomerCompany?.Name ?? string.Empty,
        r.TripId,
        r.TransportOrderId,
        r.Amount,
        r.DueDate.ToString("yyyy-MM-dd"),
        r.ReceivedDate?.ToString("yyyy-MM-dd"),
        r.Status.ToString());
}
