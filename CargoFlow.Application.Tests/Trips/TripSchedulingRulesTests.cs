using CargoFlow.Application.Trips;
using CargoFlow.Domain.Entities.Drivers;
using CargoFlow.Domain.Entities.Fleet;
using CargoFlow.Domain.Entities.TransportOrders;

namespace CargoFlow.Application.Tests.Trips;

public class TripSchedulingRulesTests
{
    private readonly TripSchedulingRules _sut = new();

    private static Driver ValidDriver() => new()
    {
        Name = "Motorista Teste",
        CnhExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
        AvailabilityStatus = DriverAvailabilityStatus.Disponivel,
    };

    private static Vehicle ValidVehicle() => new()
    {
        PlateNumber = "ABC-1234",
        Status = VehicleStatus.Disponivel,
        CapacityKg = 10000,
    };

    // Regra 1: CNH vencida não pode ser escalada.
    [Fact]
    public void ValidateDriverEligibility_Throws_WhenCnhExpiredBeforeScheduledDeparture()
    {
        var driver = ValidDriver();
        driver.CnhExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            _sut.ValidateDriverEligibility(driver, DateTime.UtcNow, hasBlockingExpiredDocuments: false));
        Assert.Contains("CNH vencida", ex.Message);
    }

    [Fact]
    public void ValidateDriverEligibility_Throws_WhenDriverNotAvailable()
    {
        var driver = ValidDriver();
        driver.AvailabilityStatus = DriverAvailabilityStatus.Ferias;

        Assert.Throws<InvalidOperationException>(() =>
            _sut.ValidateDriverEligibility(driver, DateTime.UtcNow, hasBlockingExpiredDocuments: false));
    }

    [Fact]
    public void ValidateDriverEligibility_Throws_WhenBlockingDocumentExpired()
    {
        var driver = ValidDriver();

        Assert.Throws<InvalidOperationException>(() =>
            _sut.ValidateDriverEligibility(driver, DateTime.UtcNow, hasBlockingExpiredDocuments: true));
    }

    [Fact]
    public void ValidateDriverEligibility_Succeeds_WhenDriverIsFullyEligible()
    {
        var driver = ValidDriver();
        _sut.ValidateDriverEligibility(driver, DateTime.UtcNow, hasBlockingExpiredDocuments: false);
    }

    // Regra 2: veículo indisponível não pode ser usado.
    [Fact]
    public void ValidateVehicleEligibility_Throws_WhenVehicleNotAvailable()
    {
        var vehicle = ValidVehicle();
        vehicle.Status = VehicleStatus.EmManutencao;

        Assert.Throws<InvalidOperationException>(() =>
            _sut.ValidateVehicleEligibility(vehicle, hasBlockingExpiredDocuments: false));
    }

    [Fact]
    public void ValidateVehicleEligibility_Succeeds_WhenAvailableAndCompliant()
    {
        var vehicle = ValidVehicle();
        _sut.ValidateVehicleEligibility(vehicle, hasBlockingExpiredDocuments: false);
    }

    // Regra 3: veículo incompatível (capacidade insuficiente) não pode ser programado.
    [Fact]
    public void ValidateVehicleCompatibility_Throws_WhenCapacityBelowCargoWeight()
    {
        var vehicle = ValidVehicle();
        vehicle.CapacityKg = 1000;
        var order = new TransportOrder { CargoWeightKg = 5000 };

        Assert.Throws<InvalidOperationException>(() => _sut.ValidateVehicleCompatibility(vehicle, order));
    }

    [Fact]
    public void ValidateVehicleCompatibility_Succeeds_WhenCapacitySufficient()
    {
        var vehicle = ValidVehicle();
        var order = new TransportOrder { CargoWeightKg = 5000 };

        _sut.ValidateVehicleCompatibility(vehicle, order);
    }
}
