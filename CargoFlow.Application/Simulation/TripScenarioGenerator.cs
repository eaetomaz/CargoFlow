using CargoFlow.Application.Fleet;
using CargoFlow.Application.Fuelings;
using CargoFlow.Application.Occurrences;
using CargoFlow.Application.Trips;
using CargoFlow.Domain.Entities.Occurrences;
using CargoFlow.Domain.Entities.Trips;
using Microsoft.Extensions.DependencyInjection;

namespace CargoFlow.Application.Simulation;

// Constrói o roteiro de uma viagem simulada mirando o exemplo da seção 7 do
// documento de contexto (saiu -> abasteceu -> talvez uma ocorrência -> voltou
// -> entregou -> faturou). Cada passo chama o MESMO Application service que
// a UI usa -- nenhum dado fake à parte, só orquestração de quando cada
// mutação real acontece.
public static class TripScenarioGenerator
{
    private static readonly string[] Locations =
        ["BR-101 km 220", "BR-116 km 88", "Rodovia Anhanguera km 45", "BR-040 km 502", "BR-262 km 130", "Rodovia Régis Bittencourt km 310"];

    private static readonly OccurrenceType[] MinorOccurrenceTypes =
        [OccurrenceType.PneuFurado, OccurrenceType.Atraso, OccurrenceType.DocumentacaoIrregular];

    public static List<SimulationStep> BuildSteps(TripDto trip, DateTime departureAt, Random random)
    {
        var tripId = trip.Id;
        var steps = new List<SimulationStep>();

        var totalHours = 5 + random.NextDouble() * 2; // 5-7h virtuais, mesma ordem de grandeza do exemplo do doc
        var arrivalAt = departureAt.AddHours(totalHours);

        steps.Add(new SimulationStep(departureAt, tripId, "Iniciar viagem", async (sp, now, ct) =>
        {
            var tripService = sp.GetRequiredService<ITripService>();
            var started = await tripService.StartAsync(tripId, ct);
            var ev = new SimulationEventDto("TripStarted", now.ToString("HH:mm"),
                $"Veículo {started.VehiclePlate} saiu para viagem ({started.OriginCity} → {started.DestinationCity}).", tripId);
            return new SimulationStepResult(ev, []);
        }));

        var fuelingAt = departureAt.AddHours(totalHours * (0.3 + random.NextDouble() * 0.2));
        steps.Add(new SimulationStep(fuelingAt, tripId, "Abastecer", async (sp, now, ct) =>
        {
            var vehicleRepo = sp.GetRequiredService<IVehicleRepository>();
            var fuelingService = sp.GetRequiredService<IFuelingService>();

            var vehicle = await vehicleRepo.GetByIdAsync(trip.VehicleId, ct);
            if (vehicle is null)
                return new SimulationStepResult(null, []);

            var distanceCovered = (decimal)(trip.PlannedDistanceKm > 0 ? (double)trip.PlannedDistanceKm : 500) * 0.4m;
            var consumptionKmPerLiter = 2.0m + (decimal)random.NextDouble() * 1.5m;
            var liters = Math.Round(distanceCovered / consumptionKmPerLiter, 2);
            var newReading = Math.Round(vehicle.Odometer + distanceCovered, 1);

            var fueling = await fuelingService.CreateAsync(new CreateFuelingRequest(
                trip.VehicleId, trip.DriverId, tripId, "Posto da Simulação",
                DateOnly.FromDateTime(now).ToString("yyyy-MM-dd"),
                liters, Math.Round(liters * (5.6m + (decimal)random.NextDouble() * 0.8m), 2), newReading), ct);

            var ev = new SimulationEventDto("FuelingRegistered", now.ToString("HH:mm"),
                $"Abastecimento registrado: {fueling.LiterQuantity:N1}L em {vehicle.PlateNumber}.", tripId);
            return new SimulationStepResult(ev, []);
        }));

        // ~35% das viagens simuladas têm uma parada não planejada -- mesma
        // proporção usada informalmente no exemplo do doc ("Parada não
        // planejada" nem sempre acontece numa viagem real).
        if (random.NextDouble() < 0.35)
        {
            var occurrenceAt = departureAt.AddHours(totalHours * (0.55 + random.NextDouble() * 0.15));
            var type = MinorOccurrenceTypes[random.Next(MinorOccurrenceTypes.Length)];
            var location = Locations[random.Next(Locations.Length)];

            steps.Add(new SimulationStep(occurrenceAt, tripId, "Ocorrência", async (sp, now, ct) =>
            {
                var occurrenceService = sp.GetRequiredService<IOccurrenceService>();

                var occurrence = await occurrenceService.CreateAsync(new CreateOccurrenceRequest(
                    tripId, trip.VehicleId, trip.DriverId, type.ToString(), now.ToString("O"), location,
                    "Registrada automaticamente pelo simulador de operação.", null), ct);

                var resolveAt = now.AddMinutes(20 + random.Next(40));
                var followUp = new SimulationStep(resolveAt, tripId, "Resolver ocorrência", async (sp2, now2, ct2) =>
                {
                    var occService2 = sp2.GetRequiredService<IOccurrenceService>();
                    await occService2.ResolveAsync(occurrence.Id, ct2);
                    var resolvedEv = new SimulationEventDto("OccurrenceResolved", now2.ToString("HH:mm"),
                        $"Ocorrência resolvida: veículo {trip.VehiclePlate} retomou a viagem.", tripId);
                    return new SimulationStepResult(resolvedEv, []);
                });

                var ev = new SimulationEventDto("OccurrenceCreated", now.ToString("HH:mm"),
                    $"Ocorrência registrada: {type} em {location}.", tripId);
                return new SimulationStepResult(ev, [followUp]);
            }));
        }

        steps.Add(new SimulationStep(arrivalAt, tripId, "Concluir entrega", async (sp, now, ct) =>
        {
            var tripService = sp.GetRequiredService<ITripService>();
            var actualDistanceKm = trip.PlannedDistanceKm * (0.97m + (decimal)random.NextDouble() * 0.06m);

            await tripService.CompleteDeliveryAsync(tripId, new CompleteTripDeliveryRequest(Math.Round(actualDistanceKm, 1)), ct);

            // O faturamento já foi gerado de verdade por BillingGenerationHandler
            // (reage ao DeliveryCompletedEvent publicado dentro de CompleteDeliveryAsync)
            // -- este passo só narra o que já aconteceu, não mutação nova.
            var billingFollowUp = new SimulationStep(now, tripId, "Narrar faturamento", (_, now2, _) =>
                Task.FromResult(new SimulationStepResult(
                    new SimulationEventDto("BillingGenerated", now2.ToString("HH:mm"), $"Faturamento gerado pra viagem de {trip.VehiclePlate}.", tripId),
                    [])));

            var ev = new SimulationEventDto("DeliveryCompleted", now.ToString("HH:mm"), $"Entrega realizada -- veículo {trip.VehiclePlate}.", tripId);
            return new SimulationStepResult(ev, [billingFollowUp]);
        }));

        return steps.OrderBy(s => s.VirtualTime).ToList();
    }
}
