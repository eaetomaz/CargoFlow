using CargoFlow.Application.Freight;
using CargoFlow.Domain.Entities.Fleet;
using CargoFlow.Domain.Entities.Freight;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence.Repositories.Freight;

public class FreightPricingRuleRepository(CargoFlowDbContext context) : IFreightPricingRuleRepository
{
    public Task<List<FreightPricingRule>> GetAllAsync(CancellationToken cancellationToken) =>
        context.FreightPricingRules.OrderBy(r => r.VehicleType).ThenBy(r => r.CargoType).ToListAsync(cancellationToken);

    public Task<FreightPricingRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.FreightPricingRules.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<FreightPricingRule?> FindApplicableAsync(VehicleType vehicleType, CargoType cargoType, DateOnly today, CancellationToken cancellationToken)
    {
        var candidates = await context.FreightPricingRules
            .Where(r => r.VehicleType == vehicleType && (r.CargoType == cargoType || r.CargoType == null))
            .Where(r => r.EffectiveFrom <= today && (r.EffectiveTo == null || r.EffectiveTo >= today))
            .ToListAsync(cancellationToken);

        // Regra específica pro tipo de carga vence a genérica (CargoType == null).
        return candidates.OrderByDescending(r => r.CargoType != null).FirstOrDefault();
    }

    public async Task AddAsync(FreightPricingRule rule, CancellationToken cancellationToken)
    {
        context.FreightPricingRules.Add(rule);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(FreightPricingRule rule, CancellationToken cancellationToken)
    {
        context.FreightPricingRules.Remove(rule);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
