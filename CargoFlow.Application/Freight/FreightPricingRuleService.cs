using CargoFlow.Domain.Entities.Fleet;
using CargoFlow.Domain.Entities.Freight;

namespace CargoFlow.Application.Freight;

public class FreightPricingRuleService(IFreightPricingRuleRepository repository) : IFreightPricingRuleService
{
    public async Task<List<FreightPricingRuleDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var rules = await repository.GetAllAsync(cancellationToken);
        return rules.Select(ToDto).ToList();
    }

    public async Task<FreightPricingRuleDto> CreateAsync(UpsertFreightPricingRuleRequest request, CancellationToken cancellationToken)
    {
        var rule = new FreightPricingRule();
        Apply(rule, request);

        await repository.AddAsync(rule, cancellationToken);
        return ToDto(rule);
    }

    public async Task<FreightPricingRuleDto> UpdateAsync(Guid id, UpsertFreightPricingRuleRequest request, CancellationToken cancellationToken)
    {
        var rule = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Regra de preço não encontrada.");

        Apply(rule, request);
        rule.UpdateTimestamp();

        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(rule);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var rule = await repository.GetByIdAsync(id, cancellationToken);
        if (rule is null)
            return; // idempotente

        await repository.DeleteAsync(rule, cancellationToken);
    }

    private static void Apply(FreightPricingRule rule, UpsertFreightPricingRuleRequest request)
    {
        rule.VehicleType = Enum.Parse<VehicleType>(request.VehicleType);
        rule.CargoType = string.IsNullOrWhiteSpace(request.CargoType) ? null : Enum.Parse<CargoType>(request.CargoType);
        rule.PricePerKg = request.PricePerKg;
        rule.PricePerKm = request.PricePerKm;
        rule.TollPerKm = request.TollPerKm;
        rule.AdValoremPercentage = request.AdValoremPercentage;
        rule.GrisPercentage = request.GrisPercentage;
        rule.MinimumFreightValue = request.MinimumFreightValue;
        rule.EffectiveFrom = DateOnly.Parse(request.EffectiveFrom);
        rule.EffectiveTo = string.IsNullOrWhiteSpace(request.EffectiveTo) ? null : DateOnly.Parse(request.EffectiveTo);
    }

    private static FreightPricingRuleDto ToDto(FreightPricingRule r) => new(
        r.Id,
        r.VehicleType.ToString(),
        r.CargoType?.ToString(),
        r.PricePerKg,
        r.PricePerKm,
        r.TollPerKm,
        r.AdValoremPercentage,
        r.GrisPercentage,
        r.MinimumFreightValue,
        r.EffectiveFrom.ToString("yyyy-MM-dd"),
        r.EffectiveTo?.ToString("yyyy-MM-dd"));
}
