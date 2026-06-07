using KnowledgeBase.Application.DTOs.Badges;
using KnowledgeBase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KnowledgeBase.API.Controllers;

[ApiController]
[Route("badges")]
[Produces("application/json")]
public class BadgesController : ControllerBase
{
    private readonly IBadgeService _badgeService;

    public BadgesController(IBadgeService badgeService)
    {
        _badgeService = badgeService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<BadgeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BadgeDto>>> GetAll()
    {
        var badges = await _badgeService.GetAllActiveAsync();
        return Ok(badges);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BadgeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BadgeDto>> GetById(long id)
    {
        var badge = await _badgeService.GetByIdAsync(id);
        if (badge == null)
        {
            return NotFound(new { message = "勋章不存在" });
        }
        return Ok(badge);
    }

    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(typeof(List<UserBadgeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserBadgeDto>>> GetMyBadges()
    {
        var userId = GetCurrentUserId();
        var badges = await _badgeService.GetUserBadgesAsync(userId);
        return Ok(badges);
    }

    [HttpGet("user/{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<UserBadgeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserBadgeDto>>> GetUserBadges(long userId)
    {
        var badges = await _badgeService.GetUserBadgesAsync(userId);
        return Ok(badges);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(BadgeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BadgeDto>> Create([FromBody] CreateBadgeRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var badge = await _badgeService.CreateAsync(request, currentUserId);
        return CreatedAtAction(nameof(GetById), new { id = badge.Id }, badge);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateBadgeRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            await _badgeService.UpdateAsync(id, request, currentUserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
            await _badgeService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("check")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CheckAndAwardBadges()
    {
        var userId = GetCurrentUserId();
        await _badgeService.CheckAndAwardBadgesAsync(userId);
        return NoContent();
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
