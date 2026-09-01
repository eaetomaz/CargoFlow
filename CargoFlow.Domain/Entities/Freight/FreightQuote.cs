using CargoFlow.Domain.Common;
using CargoFlow.Domain.Entities.Companies;
using CargoFlow.Domain.Entities.Fleet;

namespace CargoFlow.Domain.Entities.Freight;

public enum FreightQuoteStatus
{
    Draft,
    Sent,
    Approved,
    Rejected,
    Expired,
}

// Snapshot: o breakdown é calculado e gravado na criação (FreightQuoteCalculationService),
// nunca recalculado depois -- se a FreightPricingRule mudar amanhã, cotações
// já emitidas não podem mudar de valor retroativamente.
public class FreightQuote : Entity, IAuditable
{
    public Guid CustomerCompanyId { get; set; }
    public Company? CustomerCompany { get; set; }

    public string OriginCity { get; set; } = string.Empty;
    public string OriginState { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public string DestinationState { get; set; } = string.Empty;

    public CargoType CargoType { get; set; }
    public decimal CargoWeightKg { get; set; }
    public decimal CargoValue { get; set; }
    public VehicleType RequiredVehicleType { get; set; }
    public decimal EstimatedDistanceKm { get; set; }

    public decimal FreightWeightValue { get; set; }
    public decimal TollValue { get; set; }
    public decimal AdValoremValue { get; set; }
    public decimal GrisValue { get; set; }
    public decimal OtherCostsValue { get; set; }
    public decimal TotalValue { get; set; }

    public FreightQuoteStatus Status { get; set; } = FreightQuoteStatus.Sent;
    public DateTime ExpiresAt { get; set; }

    public void RecomputeExpiry(DateTime now)
    {
        if (Status == FreightQuoteStatus.Sent && ExpiresAt < now)
            Status = FreightQuoteStatus.Expired;
    }
}
