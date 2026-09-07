using CargoFlow.Application.Drivers;
using CargoFlow.Application.Fleet;
using CargoFlow.Application.Trips;
using CargoFlow.Domain.Entities.Fuelings;
using CargoFlow.Domain.Entities.Trips;

namespace CargoFlow.Application.Fuelings;

public class FuelingService(
    IFuelingRepository repository,
    IVehicleRepository vehicleRepository,
    IDriverRepository driverRepository,
    ITripRepository tripRepository,
    IFuelingOdometerValidator odometerValidator) : IFuelingService
{
    public async Task<List<FuelingDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var fuelings = await repository.GetAllAsync(cancellationToken);
        return fuelings.Select(ToDto).ToList();
    }

    public async Task<FuelingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var fueling = await repository.GetByIdAsync(id, cancellationToken);
        return fueling is null ? null : ToDto(fueling);
    }

    // O abastecimento também é um checkpoint de odômetro: valida a
    // coerência (regra 8) contra o odômetro atual do veículo e, se válido,
    // já atualiza o odômetro do veículo pra essa leitura.
    public async Task<FuelingDto> CreateAsync(CreateFuelingRequest request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken)
            ?? throw new InvalidOperationException("Veículo não encontrado.");

        odometerValidator.ValidateCoherence(vehicle.Odometer, request.OdometerReading);
        vehicle.Odometer = request.OdometerReading;

        var driver = request.DriverId is { } driverId
            ? await driverRepository.GetByIdAsync(driverId, cancellationToken) ?? throw new InvalidOperationException("Motorista não encontrado.")
            : null;

        var fueling = new Fueling
        {
            VehicleId = request.VehicleId,
            Vehicle = vehicle,
            DriverId = request.DriverId,
            Driver = driver,
            TripId = request.TripId,
            GasStationName = request.GasStationName,
            FuelingDate = DateOnly.Parse(request.FuelingDate),
            LiterQuantity = request.LiterQuantity,
            TotalValue = request.TotalValue,
            OdometerReading = request.OdometerReading,
        };

        await repository.AddAsync(fueling, cancellationToken);

        if (request.TripId is { } tripId)
        {
            var trip = await tripRepository.GetByIdAsync(tripId, cancellationToken)
                ?? throw new InvalidOperationException("Viagem vinculada não encontrada.");

            trip.AppendEvent(TripEventType.FuelingRegistered, DateTime.UtcNow, $"Abastecimento registrado: {request.LiterQuantity:N1}L em {request.GasStationName}.");
            await tripRepository.SaveChangesAsync(cancellationToken);
        }

        return ToDto(fueling);
    }

    // Menos de 2 abastecimentos não dá pra calcular consumo -- retorna
    // nulo/zero em vez de estourar exceção (não é um estado inválido).
    public async Task<FuelEfficiencyDto> GetFuelEfficiencyAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        var fuelings = await repository.GetByVehicleIdAsync(vehicleId, cancellationToken);
        var ordered = fuelings.OrderBy(f => f.FuelingDate).ThenBy(f => f.OdometerReading).ToList();

        if (ordered.Count < 2)
            return new FuelEfficiencyDto(vehicleId, null, null);

        var ratios = new List<decimal>();
        var costPerKms = new List<decimal>();

        for (var i = 1; i < ordered.Count; i++)
        {
            var distance = ordered[i].OdometerReading - ordered[i - 1].OdometerReading;
            if (distance <= 0 || ordered[i].LiterQuantity <= 0)
                continue;

            ratios.Add(distance / ordered[i].LiterQuantity);
            costPerKms.Add(ordered[i].TotalValue / distance);
        }

        if (ratios.Count == 0)
            return new FuelEfficiencyDto(vehicleId, null, null);

        return new FuelEfficiencyDto(vehicleId, Math.Round(ratios.Average(), 2), Math.Round(costPerKms.Average(), 2));
    }

    private static FuelingDto ToDto(Fueling f) => new(
        f.Id,
        f.VehicleId,
        f.Vehicle?.PlateNumber ?? string.Empty,
        f.DriverId,
        f.Driver?.Name,
        f.TripId,
        f.GasStationName,
        f.FuelingDate.ToString("yyyy-MM-dd"),
        f.LiterQuantity,
        f.TotalValue,
        f.OdometerReading);
}
