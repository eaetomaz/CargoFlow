using CargoFlow.Domain.Entities.Fleet;

namespace CargoFlow.Application.Fleet;

public class VehicleService(IVehicleRepository repository) : IVehicleService
{
    public async Task<List<VehicleDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var vehicles = await repository.GetAllAsync(cancellationToken);
        return vehicles.Select(ToDto).ToList();
    }

    public async Task<VehicleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var vehicle = await repository.GetByIdAsync(id, cancellationToken);
        return vehicle is null ? null : ToDto(vehicle);
    }

    public async Task<VehicleDto> CreateAsync(UpsertVehicleRequest request, CancellationToken cancellationToken)
    {
        if (await repository.PlateNumberExistsAsync(request.PlateNumber, null, cancellationToken))
            throw new InvalidOperationException($"Já existe um veículo cadastrado com a placa {request.PlateNumber}.");

        if (await repository.RenavamExistsAsync(request.Renavam, null, cancellationToken))
            throw new InvalidOperationException($"Já existe um veículo cadastrado com o Renavam {request.Renavam}.");

        var vehicle = new Vehicle();
        Apply(vehicle, request);

        await repository.AddAsync(vehicle, cancellationToken);
        return ToDto(vehicle);
    }

    public async Task<VehicleDto> UpdateAsync(Guid id, UpsertVehicleRequest request, CancellationToken cancellationToken)
    {
        var vehicle = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Veículo não encontrado.");

        if (await repository.PlateNumberExistsAsync(request.PlateNumber, id, cancellationToken))
            throw new InvalidOperationException($"Já existe outro veículo cadastrado com a placa {request.PlateNumber}.");

        if (await repository.RenavamExistsAsync(request.Renavam, id, cancellationToken))
            throw new InvalidOperationException($"Já existe outro veículo cadastrado com o Renavam {request.Renavam}.");

        Apply(vehicle, request);
        vehicle.UpdateTimestamp();

        await repository.SaveChangesAsync(cancellationToken);
        return ToDto(vehicle);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var vehicle = await repository.GetByIdAsync(id, cancellationToken);
        if (vehicle is null)
            return; // idempotente

        await repository.DeleteAsync(vehicle, cancellationToken);
    }

    private static void Apply(Vehicle vehicle, UpsertVehicleRequest request)
    {
        vehicle.PlateNumber = request.PlateNumber.Trim().ToUpperInvariant();
        vehicle.Renavam = request.Renavam.Trim();
        vehicle.Brand = request.Brand.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.ManufactureYear = request.ManufactureYear;
        vehicle.ModelYear = request.ModelYear;
        vehicle.Type = Enum.Parse<VehicleType>(request.Type);
        vehicle.AxleCount = request.AxleCount;
        vehicle.CapacityKg = request.CapacityKg;
        vehicle.TareWeightKg = request.TareWeightKg;
        vehicle.Odometer = request.Odometer;
        vehicle.Status = Enum.Parse<VehicleStatus>(request.Status);
    }

    private static VehicleDto ToDto(Vehicle v) => new(
        v.Id,
        v.PlateNumber,
        v.Renavam,
        v.Brand,
        v.Model,
        v.ManufactureYear,
        v.ModelYear,
        v.Type.ToString(),
        v.AxleCount,
        v.CapacityKg,
        v.TareWeightKg,
        v.Odometer,
        v.Status.ToString());
}
