using System.Threading.Channels;
using CargoFlow.Application.Drivers;
using CargoFlow.Application.Fleet;
using CargoFlow.Application.TransportOrders;
using CargoFlow.Application.Trips;
using CargoFlow.Domain.Entities.Drivers;
using CargoFlow.Domain.Entities.Fleet;
using CargoFlow.Domain.Entities.TransportOrders;
using Microsoft.Extensions.DependencyInjection;

namespace CargoFlow.Application.Simulation;

// Singleton -- é o único jeito de manter o estado da simulação (relógio
// virtual, fila de passos, assinantes SSE) vivo entre requisições HTTP
// independentes. Cria seu próprio escopo de DI a cada operação que precisa
// de serviços scoped (repositórios, DbContext), nunca guarda um escopo.
public class SimulationEngine(IServiceScopeFactory scopeFactory) : ISimulationEngine
{
    private const int MaxConcurrentTrips = 4;

    private readonly Lock _lock = new();
    private readonly List<SimulationStep> _pendingSteps = [];
    private readonly List<Channel<SimulationEventDto>> _subscribers = [];

    private SimulationRunStatus _status = SimulationRunStatus.Idle;
    private DateTime _virtualClock;
    private DateTime? _startedAtVirtual;
    private int _speedMultiplier = 60;
    private int _scheduledTripCount;
    private int _completedTripCount;
    private DateTime _lastTickRealTime;

    public async Task<SimulationStatusDto> StartAsync(int speedMultiplier, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (_status == SimulationRunStatus.Running)
                throw new InvalidOperationException("Já existe uma simulação em andamento.");
        }

        using var scope = scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;
        var orderRepository = sp.GetRequiredService<ITransportOrderRepository>();
        var vehicleRepository = sp.GetRequiredService<IVehicleRepository>();
        var driverRepository = sp.GetRequiredService<IDriverRepository>();
        var schedulingService = sp.GetRequiredService<ITripSchedulingService>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var orders = (await orderRepository.GetAllAsync(cancellationToken))
            .Where(o => o.Status == TransportOrderStatus.Criada)
            .Take(MaxConcurrentTrips)
            .ToList();

        if (orders.Count == 0)
            throw new InvalidOperationException("Nenhuma ordem de transporte \"Criada\" disponível pra simular. Aprove uma cotação e gere uma OT antes de iniciar.");

        var availableVehicles = (await vehicleRepository.GetAllAsync(cancellationToken))
            .Where(v => v.Status == VehicleStatus.Disponivel)
            .OrderByDescending(v => v.CapacityKg)
            .ToList();

        var availableDrivers = (await driverRepository.GetAllAsync(cancellationToken))
            .Where(d => d.AvailabilityStatus == DriverAvailabilityStatus.Disponivel && d.CnhExpiryDate > today)
            .ToList();

        var virtualStart = DateTime.UtcNow;
        var random = new Random();
        var newSteps = new List<SimulationStep>();
        var scheduledCount = 0;

        foreach (var order in orders)
        {
            var vehicle = availableVehicles.FirstOrDefault(v => v.CapacityKg >= order.CargoWeightKg);
            if (vehicle is null)
                continue;

            var driver = availableDrivers.Count > 0 ? availableDrivers[0] : null;
            if (driver is null)
                break; // sem motorista sobrando -- não adianta continuar tentando as próximas OTs

            availableVehicles.Remove(vehicle);
            availableDrivers.Remove(driver);

            // Sem integração de geolocalização real (fora do escopo, ver plano) --
            // mesma decisão pragmática da cotação: distância estimada plausível.
            var plannedDistanceKm = Math.Round(300 + (decimal)random.NextDouble() * 900, 1);

            TripDto trip;
            try
            {
                trip = await schedulingService.ScheduleTripAsync(new ScheduleTripRequest(
                    order.Id, vehicle.Id, null, driver.Id,
                    virtualStart.ToString("O"), virtualStart.AddHours(7).ToString("O"),
                    plannedDistanceKm), cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // Elegibilidade falhou (ex: documento venceu entre a seleção acima e
                // agora) -- devolve os recursos pro pool e tenta a próxima OT.
                availableVehicles.Add(vehicle);
                availableDrivers.Add(driver);
                continue;
            }

            newSteps.AddRange(TripScenarioGenerator.BuildSteps(trip, virtualStart, random));
            scheduledCount++;
        }

        if (scheduledCount == 0)
            throw new InvalidOperationException("Nenhuma OT pôde ser programada agora -- falta veículo ou motorista disponível e elegível (CNH/documentos em dia).");

        lock (_lock)
        {
            _pendingSteps.Clear();
            _pendingSteps.AddRange(newSteps);
            _status = SimulationRunStatus.Running;
            _virtualClock = virtualStart;
            _startedAtVirtual = virtualStart;
            _speedMultiplier = speedMultiplier;
            _scheduledTripCount = scheduledCount;
            _completedTripCount = 0;
            _lastTickRealTime = DateTime.UtcNow;
        }

        Publish(new SimulationEventDto("SimulationStarted", virtualStart.ToString("HH:mm"),
            $"Simulação iniciada com {scheduledCount} viagem(ns) -- velocidade {speedMultiplier}x.", null));

        return GetStatus();
    }

    public SimulationStatusDto Stop()
    {
        lock (_lock)
        {
            if (_status != SimulationRunStatus.Running)
                throw new InvalidOperationException("Não há simulação em andamento.");

            _status = SimulationRunStatus.Finished;
            _pendingSteps.Clear();
        }

        Publish(new SimulationEventDto("SimulationStopped", _virtualClock.ToString("HH:mm"), "Simulação interrompida manualmente.", null));
        return GetStatus();
    }

    public SimulationStatusDto GetStatus()
    {
        lock (_lock)
        {
            return new SimulationStatusDto(
                _status.ToString(),
                _startedAtVirtual?.ToString("O"),
                _status == SimulationRunStatus.Idle ? null : _virtualClock.ToString("O"),
                _speedMultiplier,
                _scheduledTripCount,
                _completedTripCount,
                _pendingSteps.Count);
        }
    }

    public async Task TickAsync(CancellationToken cancellationToken)
    {
        List<SimulationStep> due;
        DateTime virtualNow;

        lock (_lock)
        {
            if (_status != SimulationRunStatus.Running)
                return;

            var realElapsed = DateTime.UtcNow - _lastTickRealTime;
            _lastTickRealTime = DateTime.UtcNow;
            _virtualClock = _virtualClock.Add(realElapsed * _speedMultiplier);
            virtualNow = _virtualClock;

            due = _pendingSteps.Where(s => s.VirtualTime <= virtualNow).ToList();
            foreach (var step in due)
                _pendingSteps.Remove(step);
        }

        if (due.Count > 0)
        {
            using var scope = scopeFactory.CreateScope();
            foreach (var step in due.OrderBy(s => s.VirtualTime))
            {
                try
                {
                    var result = await step.Execute(scope.ServiceProvider, virtualNow, cancellationToken);

                    if (result.Event is not null)
                        Publish(result.Event);

                    if (result.FollowUpSteps.Count > 0)
                        lock (_lock)
                            _pendingSteps.AddRange(result.FollowUpSteps);

                    if (step.Description == "Concluir entrega")
                        lock (_lock)
                            _completedTripCount++;
                }
                catch (Exception ex)
                {
                    Publish(new SimulationEventDto("SimulationStepFailed", virtualNow.ToString("HH:mm"),
                        $"Falha num passo simulado ({step.Description}): {ex.Message}", step.TripId));
                }
            }
        }

        CheckForCompletion();
    }

    private void CheckForCompletion()
    {
        var finished = false;
        lock (_lock)
        {
            if (_status == SimulationRunStatus.Running && _pendingSteps.Count == 0)
            {
                _status = SimulationRunStatus.Finished;
                finished = true;
            }
        }

        if (finished)
            Publish(new SimulationEventDto("SimulationFinished", _virtualClock.ToString("HH:mm"),
                "Simulação concluída -- todas as viagens chegaram ao destino.", null));
    }

    public ChannelReader<SimulationEventDto> Subscribe()
    {
        var channel = Channel.CreateBounded<SimulationEventDto>(new BoundedChannelOptions(200) { FullMode = BoundedChannelFullMode.DropOldest });
        lock (_lock)
            _subscribers.Add(channel);
        return channel.Reader;
    }

    public void Unsubscribe(ChannelReader<SimulationEventDto> reader)
    {
        lock (_lock)
            _subscribers.RemoveAll(c => c.Reader == reader);
    }

    private void Publish(SimulationEventDto ev)
    {
        List<Channel<SimulationEventDto>> snapshot;
        lock (_lock)
            snapshot = [.. _subscribers];

        foreach (var channel in snapshot)
            channel.Writer.TryWrite(ev);
    }
}
