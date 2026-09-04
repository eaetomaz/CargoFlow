namespace CargoFlow.Application.Freight;

public interface IFreightQuoteService
{
    Task<FreightQuoteBreakdown> CalculateAsync(CalculateFreightQuoteRequest request, CancellationToken cancellationToken);
    Task<List<FreightQuoteDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<FreightQuoteDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<FreightQuoteDto> CreateAsync(CreateFreightQuoteRequest request, CancellationToken cancellationToken);
    Task<FreightQuoteDto> ApproveAsync(Guid id, CancellationToken cancellationToken);
    Task<FreightQuoteDto> RejectAsync(Guid id, CancellationToken cancellationToken);
}
