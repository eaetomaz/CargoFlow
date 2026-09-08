using CargoFlow.Application.Companies;
using CargoFlow.Domain.Entities.Finance;

namespace CargoFlow.Application.Finance;

public class AccountPayableService(IAccountPayableRepository repository, ICompanyRepository companyRepository) : IAccountPayableService
{
    public async Task<List<AccountPayableDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var payables = await repository.GetAllAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var changed = false;
        foreach (var p in payables)
        {
            var before = p.Status;
            p.RecomputeOverdue(today);
            if (p.Status != before) changed = true;
        }
        if (changed) await repository.SaveChangesAsync(cancellationToken);

        return payables.Select(ToDto).ToList();
    }

    public async Task<AccountPayableDto> CreateAsync(CreateAccountPayableRequest request, CancellationToken cancellationToken)
    {
        Domain.Entities.Companies.Company? supplier = null;
        if (request.SupplierCompanyId is { } supplierId)
            supplier = await companyRepository.GetByIdAsync(supplierId, cancellationToken)
                ?? throw new InvalidOperationException("Fornecedor não encontrado.");

        var payable = new AccountPayable
        {
            Category = Enum.Parse<AccountPayableCategory>(request.Category),
            Description = request.Description,
            SupplierCompanyId = request.SupplierCompanyId,
            SourceType = request.SourceType,
            SourceId = request.SourceId,
            Amount = request.Amount,
            DueDate = DateOnly.Parse(request.DueDate),
        };

        await repository.AddAsync(payable, cancellationToken);
        payable.SupplierCompany = supplier;
        return ToDto(payable);
    }

    public async Task<AccountPayableDto> PayAsync(Guid id, CancellationToken cancellationToken)
    {
        var payable = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Conta a pagar não encontrada.");

        payable.Pay(DateOnly.FromDateTime(DateTime.UtcNow));
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(payable);
    }

    public async Task<AccountPayableDto> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var payable = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Conta a pagar não encontrada.");

        payable.Cancel();
        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(payable);
    }

    private static AccountPayableDto ToDto(AccountPayable p) => new(
        p.Id,
        p.Category.ToString(),
        p.Description,
        p.SupplierCompanyId,
        p.SupplierCompany?.Name,
        p.SourceType,
        p.SourceId,
        p.Amount,
        p.DueDate.ToString("yyyy-MM-dd"),
        p.PaidDate?.ToString("yyyy-MM-dd"),
        p.Status.ToString());
}
