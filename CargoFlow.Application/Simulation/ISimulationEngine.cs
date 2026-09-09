using System.Threading.Channels;

namespace CargoFlow.Application.Simulation;

public interface ISimulationEngine
{
    Task<SimulationStatusDto> StartAsync(int speedMultiplier, CancellationToken cancellationToken);
    SimulationStatusDto Stop();
    SimulationStatusDto GetStatus();

    // Chamado periodicamente pelo SimulationHostedService -- avança o
    // relógio virtual e executa os passos que já venceram.
    Task TickAsync(CancellationToken cancellationToken);

    ChannelReader<SimulationEventDto> Subscribe();
    void Unsubscribe(ChannelReader<SimulationEventDto> reader);
}
