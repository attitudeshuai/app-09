using KnowledgeBase.Application.DTOs.Follows;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KnowledgeBase.API.Controllers;

[ApiController]
[Route("follows")]
[Authorize]
[Produces("application/json")]
public class FollowsController : ControllerBase
{
    private readonly IFollowService _followService;

    public FollowsController(IFollowService followService)
    {
        _followService = followService;
    }

    [HttpGet("{userId}/status")]
    [ProducesResponseType(typeof(FollowStatusDto), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FollowStatusDto>> GetFollowStatus(long userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var status = await _followService.GetFollowStatusAsync(currentUserId, userId);
            return Ok(status);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{userId}")]
    [ProducesResponseType(typeof(FollowStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FollowStatusDto>> ToggleFollow(long userId)
    {
        try
        {
            var followerId = GetCurrentUserId();
            var result = await _followService.ToggleFollowAsync(followerId, userId);
            return Ok(result);
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

    [HttpGet("{userId}/followers")]
    [ProducesResponseType(typeof(List<FollowerUserDto>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<FollowerUserDto>>> GetFollowers(long userId)
    {
        try
        {
            var followers = await _followService.GetFollowersAsync(userId);
            return Ok(followers);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{userId}/following")]
    [ProducesResponseType(typeof(List<FollowingUserDto>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<FollowingUserDto>>> GetFollowing(long userId)
    {
        try
        {
            var following = await _followService.GetFollowingAsync(userId);
            return Ok(following);
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
