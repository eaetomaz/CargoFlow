namespace CargoFlow.Application.Finance;

public interface IAccountPayableService
{
    Task<List<AccountPayableDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<AccountPayableDto> CreateAsync(CreateAccountPayableRequest request, CancellationToken cancellationToken);
    Task<AccountPayableDto> PayAsync(Guid id, CancellationToken cancellationToken);
    Task<AccountPayableDto> CancelAsync(Guid id, CancellationToken cancellationToken);
}
