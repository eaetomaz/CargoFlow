using CargoFlow.Domain.Common;
using CargoFlow.Domain.Entities.Drivers;
using CargoFlow.Domain.Entities.Fleet;

namespace CargoFlow.Domain.Entities.Fuelings;

public class Fueling : Entity, IAuditable
{
    public Guid VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }
    public Guid? TripId { get; set; }
    public string GasStationName { get; set; } = string.Empty;
    public DateOnly FuelingDate { get; set; }
    public decimal LiterQuantity { get; set; }
    public decimal TotalValue { get; set; }
    public decimal OdometerReading { get; set; }
}
