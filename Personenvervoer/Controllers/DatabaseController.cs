using Microsoft.AspNetCore.Mvc;
using Personenvervoer.Services;

namespace Personenvervoer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public DatabaseController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        try
        {
            using var conn = _databaseService.OpenConnection();
            return Ok(new { status = "ok", database = "connected" });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new { status = "error", message = ex.Message });
        }
    }
}