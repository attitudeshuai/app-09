using KnowledgeBase.Application.DTOs.Levels;
using KnowledgeBase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KnowledgeBase.API.Controllers;

[ApiController]
[Route("levels")]
[Produces("application/json")]
public class LevelsController : ControllerBase
{
    private readonly ILevelService _levelService;

    public LevelsController(ILevelService levelService)
    {
        _levelService = levelService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<LevelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LevelDto>>> GetAll()
    {
        var levels = await _levelService.GetAllAsync();
        return Ok(levels);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LevelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LevelDto>> GetById(long id)
    {
        var level = await _levelService.GetByIdAsync(id);
        if (level == null)
        {
            return NotFound(new { message = "等级不存在" });
        }
        return Ok(level);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(LevelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LevelDto>> Create([FromBody] CreateLevelRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var level = await _levelService.CreateAsync(request, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = level.Id }, level);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateLevelRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            await _levelService.UpdateAsync(id, request, currentUserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _levelService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
