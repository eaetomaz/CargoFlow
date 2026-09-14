using CargoFlow.Application.Fuelings;

namespace CargoFlow.Application.Tests.Fuelings;

public class FuelingOdometerValidatorTests
{
    private readonly FuelingOdometerValidator _sut = new();

    [Fact]
    public void ValidateCoherence_Throws_WhenReadingBelowCurrentOdometer()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => _sut.ValidateCoherence(currentVehicleOdometer: 50000m, newReading: 49000m));
        Assert.Contains("odômetro", ex.Message);
    }

    [Fact]
    public void ValidateCoherence_Succeeds_WhenReadingEqualsCurrentOdometer()
    {
        _sut.ValidateCoherence(currentVehicleOdometer: 50000m, newReading: 50000m);
    }

    [Fact]
    public void ValidateCoherence_Succeeds_WhenReadingAboveCurrentOdometer()
    {
        _sut.ValidateCoherence(currentVehicleOdometer: 50000m, newReading: 50350m);
    }
}
