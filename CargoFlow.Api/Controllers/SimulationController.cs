using System.Text.Json;
using CargoFlow.Application.Simulation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/simulation")]
public class SimulationController(ISimulationEngine engine, IConfiguration configuration) : ControllerBase
{
    private static readonly JsonSerializerOptions SseJsonOptions = new(JsonSerializerDefaults.Web);

    [HttpPost("start")]
    [Authorize(Roles = "Administrador,Operacional")]
    public async Task<ActionResult<SimulationStatusDto>> Start([FromBody] StartSimulationRequest? request, CancellationToken cancellationToken)
    {
        var speedMultiplier = request?.SpeedMultiplier ?? configuration.GetValue("Simulation:DefaultSpeedMultiplier", 60);

        try
        {
            return Ok(await engine.StartAsync(speedMultiplier, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("stop")]
    [Authorize(Roles = "Administrador,Operacional")]
    public ActionResult<SimulationStatusDto> Stop()
    {
        try
        {
            return Ok(engine.Stop());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("status")]
    public ActionResult<SimulationStatusDto> GetStatus() => Ok(engine.GetStatus());

    // SSE simples (sem pacote extra) -- consumido pelo front via fetch +
    // ReadableStream, não pelo EventSource nativo do navegador, porque
    // EventSource não permite mandar o header Authorization: Bearer.
    [HttpGet("stream")]
    public async Task Stream(CancellationToken cancellationToken)
    {
        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";

        var reader = engine.Subscribe();
        try
        {
            await WriteEventAsync("status", engine.GetStatus(), cancellationToken);

            while (await reader.WaitToReadAsync(cancellationToken))
            {
                while (reader.TryRead(out var simulationEvent))
                    await WriteEventAsync("event", simulationEvent, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Cliente desconectou (fechou a aba, navegou pra outra página) -- normal.
        }
        finally
        {
            engine.Unsubscribe(reader);
        }
    }

    private async Task WriteEventAsync<T>(string eventName, T payload, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload, SseJsonOptions);
        await Response.WriteAsync($"event: {eventName}\ndata: {json}\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}
