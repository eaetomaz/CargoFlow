using CargoFlow.Application.Simulation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CargoFlow.Infrastructure.Simulation;

// Só dá o "tick" -- toda a lógica de quando avançar o relógio virtual e
// quais passos executar vive no SimulationEngine (singleton). Roda dentro
// da própria Api (sem projeto Workers separado, ver decisão do plano) porque
// precisa empurrar eventos ao vivo pro mesmo processo que serve o SSE.
public class SimulationHostedService(ISimulationEngine engine, ILogger<SimulationHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TickInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await engine.TickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro no tick do simulador de operação.");
            }
        }
    }
}
