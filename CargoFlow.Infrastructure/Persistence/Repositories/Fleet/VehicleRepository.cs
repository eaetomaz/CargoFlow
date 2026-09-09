using CargoFlow.Application.Fleet;
using CargoFlow.Domain.Entities.Fleet;
using Microsoft.EntityFrameworkCore;

namespace CargoFlow.Infrastructure.Persistence.Repositories.Fleet;

public class VehicleRepository(CargoFlowDbContext context) : IVehicleRepository
{
    public Task<List<Vehicle>> GetAllAsync(CancellationToken cancellationToken) =>
        context.Vehicles.OrderBy(v => v.PlateNumber).ToListAsync(cancellationToken);

    public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Vehicles.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public Task<bool> PlateNumberExistsAsync(string plateNumber, Guid? excludingId, CancellationToken cancellationToken) =>
        context.Vehicles.AnyAsync(v => v.PlateNumber == plateNumber && (excludingId == null || v.Id != excludingId), cancellationToken);

    public Task<bool> RenavamExistsAsync(string renavam, Guid? excludingId, CancellationToken cancellationToken) =>
        context.Vehicles.AnyAsync(v => v.Renavam == renavam && (excludingId == null || v.Id != excludingId), cancellationToken);

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        context.Vehicles.Remove(vehicle);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
