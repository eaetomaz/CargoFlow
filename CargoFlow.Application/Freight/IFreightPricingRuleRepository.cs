using CargoFlow.Domain.Entities.Fleet;
using CargoFlow.Domain.Entities.Freight;

namespace CargoFlow.Application.Freight;

public interface IFreightPricingRuleRepository
{
    Task<List<FreightPricingRule>> GetAllAsync(CancellationToken cancellationToken);
    Task<FreightPricingRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    // Regra específica (VehicleType + CargoType) tem prioridade sobre a
    // genérica (CargoType == null), entre as vigentes na data informada.
    Task<FreightPricingRule?> FindApplicableAsync(VehicleType vehicleType, CargoType cargoType, DateOnly today, CancellationToken cancellationToken);

    Task AddAsync(FreightPricingRule rule, CancellationToken cancellationToken);
    Task DeleteAsync(FreightPricingRule rule, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
