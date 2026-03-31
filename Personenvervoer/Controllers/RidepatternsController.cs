using Microsoft.AspNetCore.Mvc;
using Personenvervoer.Models;
using Personenvervoer.Services;

namespace Personenvervoer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RidepatternsController : ControllerBase
{
    private readonly RidepatternService _ridepatternService;

    public RidepatternsController(RidepatternService ridepatternService)
    {
        _ridepatternService = ridepatternService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 0)
    {
        var patterns = await _ridepatternService.GetAllAsync(page);
        return Ok(patterns);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var pattern = await _ridepatternService.GetByIdAsync(id);
        if (pattern == null) return NotFound();
        return Ok(pattern);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RidepatternModel pattern)
    {
        var created = await _ridepatternService.CreateAsync(pattern);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] RidepatternModel pattern)
    {
        var updated = await _ridepatternService.UpdateAsync(id, pattern);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _ridepatternService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
