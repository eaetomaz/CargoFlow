using CargoFlow.Domain.Entities.Drivers;
using CargoFlow.Domain.Entities.Fleet;
using CargoFlow.Domain.Entities.TransportOrders;

namespace CargoFlow.Application.Trips;

// Puro -- sem dependência de banco. Recebe as entidades já carregadas (e um
// bool pré-calculado pra documentos vencidos) e só valida, lançando
// InvalidOperationException com mensagem clara. Isso é o que permite testar
// as regras 1/2/3/4 do documento de contexto sem precisar de banco de dados
// (ver TripSchedulingRulesTests) -- TripSchedulingService é quem busca os
// dados e delega a decisão pra cá.
public interface ITripSchedulingRules
{
    void ValidateDriverEligibility(Driver driver, DateTime scheduledDepartureAt, bool hasBlockingExpiredDocuments);
    void ValidateVehicleEligibility(Vehicle vehicle, bool hasBlockingExpiredDocuments);
    void ValidateVehicleCompatibility(Vehicle vehicle, TransportOrder order);
}

public class TripSchedulingRules : ITripSchedulingRules
{
    // Regra 1: motorista não pode ser escalado com CNH vencida (nem uma que
    // vença antes da data de saída programada) nem se já estiver ocupado.
    public void ValidateDriverEligibility(Driver driver, DateTime scheduledDepartureAt, bool hasBlockingExpiredDocuments)
    {
        if (driver.CnhExpiryDate < DateOnly.FromDateTime(scheduledDepartureAt))
            throw new InvalidOperationException($"Motorista {driver.Name} tem CNH vencida (ou que vence antes da saída programada) -- não pode ser escalado.");

        if (driver.AvailabilityStatus != DriverAvailabilityStatus.Disponivel)
            throw new InvalidOperationException($"Motorista {driver.Name} não está disponível (status atual: {driver.AvailabilityStatus}).");

        if (hasBlockingExpiredDocuments)
            throw new InvalidOperationException($"Motorista {driver.Name} tem documento crítico vencido -- operação bloqueada.");
    }

    // Regra 2: veículo indisponível não pode ser utilizado.
    public void ValidateVehicleEligibility(Vehicle vehicle, bool hasBlockingExpiredDocuments)
    {
        if (vehicle.Status != VehicleStatus.Disponivel)
            throw new InvalidOperationException($"Veículo {vehicle.PlateNumber} não está disponível (status atual: {vehicle.Status}).");

        if (hasBlockingExpiredDocuments)
            throw new InvalidOperationException($"Veículo {vehicle.PlateNumber} tem documento crítico vencido (CRLV/seguro) -- operação bloqueada.");
    }

    // Regra 3: veículo incompatível com a operação não pode ser programado
    // -- aqui, capacidade de carga insuficiente pro peso da OT.
    public void ValidateVehicleCompatibility(Vehicle vehicle, TransportOrder order)
    {
        if (vehicle.CapacityKg < order.CargoWeightKg)
            throw new InvalidOperationException(
                $"Veículo {vehicle.PlateNumber} tem capacidade de {vehicle.CapacityKg:N0}kg, insuficiente pros {order.CargoWeightKg:N0}kg da carga.");
    }
}
