namespace CargoFlow.Application.Fuelings;

// Puro -- sem dependência de banco, mesmo estilo de TripSchedulingRules.
// Regra 8 do documento de contexto: "abastecimentos devem respeitar
// coerência mínima de quilometragem" -- a leitura do odômetro não pode
// retroceder em relação ao valor atual do veículo.
public interface IFuelingOdometerValidator
{
    void ValidateCoherence(decimal currentVehicleOdometer, decimal newReading);
}

public class FuelingOdometerValidator : IFuelingOdometerValidator
{
    public void ValidateCoherence(decimal currentVehicleOdometer, decimal newReading)
    {
        if (newReading < currentVehicleOdometer)
            throw new InvalidOperationException(
                $"Leitura do odômetro ({newReading:N1}km) não pode ser menor que o odômetro atual do veículo ({currentVehicleOdometer:N1}km).");
    }
}
