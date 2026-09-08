namespace CargoFlow.Application.Finance;

public interface IAccountReceivableService
{
    Task<List<AccountReceivableDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<AccountReceivableDto> CreateAsync(CreateAccountReceivableRequest request, CancellationToken cancellationToken);
    Task<AccountReceivableDto> ReceiveAsync(Guid id, CancellationToken cancellationToken);
    Task<AccountReceivableDto> CancelAsync(Guid id, CancellationToken cancellationToken);
}
