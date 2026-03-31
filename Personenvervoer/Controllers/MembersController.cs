using Microsoft.AspNetCore.Mvc;
using Personenvervoer.Models;
using Personenvervoer.Services;

namespace Personenvervoer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly MemberService _memberService;

    public MembersController(MemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 0)
    {
        var members = await _memberService.GetAllAsync(page);
        return Ok(members);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var member = await _memberService.GetByIdAsync(id);
        if (member == null) return NotFound();
        return Ok(member);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MemberModel member)
    {
        var created = await _memberService.CreateAsync(member);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] MemberModel member)
    {
        var updated = await _memberService.UpdateAsync(id, member);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _memberService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
