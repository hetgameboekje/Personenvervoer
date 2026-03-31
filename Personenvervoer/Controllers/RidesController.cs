using Microsoft.AspNetCore.Mvc;
using Personenvervoer.Models;
using Personenvervoer.Services;

namespace Personenvervoer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RidesController : ControllerBase
{
    private readonly RideService _rideService;

    public RidesController(RideService rideService)
    {
        _rideService = rideService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 0)
    {
        var rides = await _rideService.GetAllAsync(page);
        return Ok(rides);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ride = await _rideService.GetByIdAsync(id);
        if (ride == null) return NotFound();
        return Ok(ride);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RideModel ride)
    {
        var created = await _rideService.CreateAsync(ride);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RideModel ride)
    {
        var updated = await _rideService.UpdateAsync(id, ride);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _rideService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // GET /api/rides/{id}/distance - Calculate distance using fake Google API
    [HttpGet("{id:guid}/distance")]
    public async Task<IActionResult> GetDistance(Guid id)
    {
        var result = await _rideService.GetRideDistanceAsync(id);
        return Ok(result);
    }
}
