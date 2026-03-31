using Microsoft.AspNetCore.Mvc;
using Personenvervoer.Models;
using Personenvervoer.Services;

namespace Personenvervoer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly LocationService _locationService;

    public LocationsController(LocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 0)
    {
        var locations = await _locationService.GetAllAsync(page);
        return Ok(locations);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var location = await _locationService.GetByIdAsync(id);
        if (location == null) return NotFound();
        return Ok(location);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LocationModel location)
    {
        var created = await _locationService.CreateAsync(location);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] LocationModel location)
    {
        var updated = await _locationService.UpdateAsync(id, location);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _locationService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
