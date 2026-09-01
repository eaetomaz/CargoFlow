using CargoFlow.Domain.Common;
using CargoFlow.Domain.Entities.Fleet;

namespace CargoFlow.Domain.Entities.Freight;

public enum CargoType
{
    Geral,
    Perecivel,
    Fragil,
    Perigosa,
    Granel,
    Refrigerada,
    Container,
}

// Tabela de preço configurável -- o objetivo (documento de contexto, seção
// 5.5) é nunca ter valor de frete hardcoded no código, só parametrizado
// aqui e lido pelo FreightQuoteCalculationService.
public class FreightPricingRule : Entity, IAuditable
{
    public VehicleType VehicleType { get; set; }

    // null = vale pra qualquer tipo de carga com esse veículo. Regras
    // específicas (CargoType preenchido) têm prioridade sobre a genérica --
    // ver FreightPricingRuleRepository.FindApplicableAsync.
    public CargoType? CargoType { get; set; }

    public decimal PricePerKg { get; set; }
    public decimal PricePerKm { get; set; }
    public decimal TollPerKm { get; set; }
    public decimal AdValoremPercentage { get; set; }
    public decimal GrisPercentage { get; set; }
    public decimal MinimumFreightValue { get; set; }

    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}
