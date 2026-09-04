namespace CargoFlow.Application.Freight;

public interface IFreightPricingRuleService
{
    Task<List<FreightPricingRuleDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<FreightPricingRuleDto> CreateAsync(UpsertFreightPricingRuleRequest request, CancellationToken cancellationToken);
    Task<FreightPricingRuleDto> UpdateAsync(Guid id, UpsertFreightPricingRuleRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
