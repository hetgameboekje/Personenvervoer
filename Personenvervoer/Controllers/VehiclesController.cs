using Microsoft.AspNetCore.Mvc;
using Personenvervoer.Models;
using Personenvervoer.Services;

namespace Personenvervoer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly VehicleService _vehicleService;

    public VehiclesController(VehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 0)
    {
        var vehicles = await _vehicleService.GetAllAsync(page);
        return Ok(vehicles);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var vehicle = await _vehicleService.GetByIdAsync(id);
        if (vehicle == null) return NotFound();
        return Ok(vehicle);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VehicleModel vehicle)
    {
        var created = await _vehicleService.CreateAsync(vehicle);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] VehicleModel vehicle)
    {
        var updated = await _vehicleService.UpdateAsync(id, vehicle);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _vehicleService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
