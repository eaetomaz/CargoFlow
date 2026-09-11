using CargoFlow.Application.Trips;
using Microsoft.AspNetCore.Mvc;

namespace CargoFlow.Api.Controllers;

[ApiController]
[Route("api/trips")]
public class TripsController(ITripService service, ITripSchedulingService schedulingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TripDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TripDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var trip = await service.GetByIdAsync(id, cancellationToken);
        return trip is null ? NotFound() : Ok(trip);
    }

    [HttpPost("schedule")]
    public async Task<ActionResult<TripDto>> Schedule([FromBody] ScheduleTripRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var trip = await schedulingService.ScheduleTripAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = trip.Id }, trip);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<TripDto>> Start(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.StartAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/expenses")]
    public async Task<ActionResult<TripDto>> AddExpense(Guid id, [FromBody] AddTripExpenseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.AddExpenseAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/complete-delivery")]
    public async Task<ActionResult<TripDto>> CompleteDelivery(Guid id, [FromBody] CompleteTripDeliveryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.CompleteDeliveryAsync(id, request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<TripDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.CancelAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/profitability")]
    public async Task<ActionResult<TripProfitabilityDto>> GetProfitability(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.GetProfitabilityAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
