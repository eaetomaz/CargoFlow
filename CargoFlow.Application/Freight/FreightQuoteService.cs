using CargoFlow.Application.Companies;
using CargoFlow.Domain.Entities.Fleet;
using CargoFlow.Domain.Entities.Freight;

namespace CargoFlow.Application.Freight;

public class FreightQuoteService(
    IFreightQuoteRepository quoteRepository,
    IFreightPricingRuleRepository pricingRuleRepository,
    ICompanyRepository companyRepository,
    IFreightQuoteCalculationService calculationService) : IFreightQuoteService
{
    public async Task<FreightQuoteBreakdown> CalculateAsync(CalculateFreightQuoteRequest request, CancellationToken cancellationToken)
    {
        var rule = await FindRuleOrThrowAsync(
            Enum.Parse<VehicleType>(request.RequiredVehicleType),
            Enum.Parse<CargoType>(request.CargoType),
            cancellationToken);

        return calculationService.Calculate(rule, request.CargoWeightKg, request.CargoValue, request.EstimatedDistanceKm, request.OtherCostsValue);
    }

    public async Task<List<FreightQuoteDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var quotes = await quoteRepository.GetAllAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var expired = false;
        foreach (var quote in quotes)
        {
            var statusBefore = quote.Status;
            quote.RecomputeExpiry(now);
            if (quote.Status != statusBefore)
                expired = true;
        }
        if (expired)
            await quoteRepository.SaveChangesAsync(cancellationToken);

        return quotes.Select(ToDto).ToList();
    }

    public async Task<FreightQuoteDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var quote = await quoteRepository.GetByIdAsync(id, cancellationToken);
        return quote is null ? null : ToDto(quote);
    }

    public async Task<FreightQuoteDto> CreateAsync(CreateFreightQuoteRequest request, CancellationToken cancellationToken)
    {
        var customer = await companyRepository.GetByIdAsync(request.CustomerCompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        var cargoType = Enum.Parse<CargoType>(request.CargoType);
        var vehicleType = Enum.Parse<VehicleType>(request.RequiredVehicleType);
        var rule = await FindRuleOrThrowAsync(vehicleType, cargoType, cancellationToken);

        var breakdown = calculationService.Calculate(rule, request.CargoWeightKg, request.CargoValue, request.EstimatedDistanceKm, request.OtherCostsValue);

        var quote = new FreightQuote
        {
            CustomerCompanyId = customer.Id,
            OriginCity = request.OriginCity,
            OriginState = request.OriginState,
            DestinationCity = request.DestinationCity,
            DestinationState = request.DestinationState,
            CargoType = cargoType,
            CargoWeightKg = request.CargoWeightKg,
            CargoValue = request.CargoValue,
            RequiredVehicleType = vehicleType,
            EstimatedDistanceKm = request.EstimatedDistanceKm,
            FreightWeightValue = breakdown.FreightWeightValue,
            TollValue = breakdown.TollValue,
            AdValoremValue = breakdown.AdValoremValue,
            GrisValue = breakdown.GrisValue,
            OtherCostsValue = breakdown.OtherCostsValue,
            TotalValue = breakdown.TotalValue,
            Status = FreightQuoteStatus.Sent,
            ExpiresAt = DateTime.UtcNow.AddDays(Math.Max(request.ValidForDays, 1)),
        };

        await quoteRepository.AddAsync(quote, cancellationToken);
        quote.CustomerCompany = customer;
        return ToDto(quote);
    }

    public async Task<FreightQuoteDto> ApproveAsync(Guid id, CancellationToken cancellationToken)
    {
        var quote = await GetSentQuoteOrThrowAsync(id, cancellationToken);
        quote.Status = FreightQuoteStatus.Approved;
        quote.UpdateTimestamp();
        await quoteRepository.SaveChangesAsync(cancellationToken);
        return ToDto(quote);
    }

    public async Task<FreightQuoteDto> RejectAsync(Guid id, CancellationToken cancellationToken)
    {
        var quote = await GetSentQuoteOrThrowAsync(id, cancellationToken);
        quote.Status = FreightQuoteStatus.Rejected;
        quote.UpdateTimestamp();
        await quoteRepository.SaveChangesAsync(cancellationToken);
        return ToDto(quote);
    }

    private async Task<FreightQuote> GetSentQuoteOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var quote = await quoteRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Cotação não encontrada.");

        quote.RecomputeExpiry(DateTime.UtcNow);
        if (quote.Status != FreightQuoteStatus.Sent)
            throw new InvalidOperationException($"Só é possível decidir cotações com status \"Sent\" -- esta está \"{quote.Status}\".");

        return quote;
    }

    private async Task<FreightPricingRule> FindRuleOrThrowAsync(VehicleType vehicleType, CargoType cargoType, CancellationToken cancellationToken) =>
        await pricingRuleRepository.FindApplicableAsync(vehicleType, cargoType, DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken)
            ?? throw new InvalidOperationException($"Nenhuma regra de preço configurada pra veículo {vehicleType}/carga {cargoType}. Cadastre uma em Personalização de preços antes de cotar.");

    private static FreightQuoteDto ToDto(FreightQuote q) => new(
        q.Id,
        q.CustomerCompanyId,
        q.CustomerCompany?.Name ?? string.Empty,
        q.OriginCity,
        q.OriginState,
        q.DestinationCity,
        q.DestinationState,
        q.CargoType.ToString(),
        q.CargoWeightKg,
        q.CargoValue,
        q.RequiredVehicleType.ToString(),
        q.EstimatedDistanceKm,
        q.FreightWeightValue,
        q.TollValue,
        q.AdValoremValue,
        q.GrisValue,
        q.OtherCostsValue,
        q.TotalValue,
        q.Status.ToString(),
        q.ExpiresAt.ToString("O"),
        q.CreatedAt.ToString("O"));
}
