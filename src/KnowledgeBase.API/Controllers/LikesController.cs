using KnowledgeBase.Application.DTOs.Likes;
using KnowledgeBase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KnowledgeBase.API.Controllers;

[ApiController]
[Route("likes")]
[Authorize]
[Produces("application/json")]
public class LikesController : ControllerBase
{
    private readonly ILikeService _likeService;

    public LikesController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    [HttpGet("{documentId}/status")]
    [ProducesResponseType(typeof(LikeStatusDto), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<ActionResult<LikeStatusDto>> GetLikeStatus(long documentId)
    {
        var userId = GetCurrentUserId();
        var status = await _likeService.GetLikeStatusAsync(userId, documentId);
        return Ok(status);
    }

    [HttpGet("{documentId}/count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<ActionResult<int>> GetLikeCount(long documentId)
    {
        var count = await _likeService.GetLikeCountAsync(documentId);
        return Ok(count);
    }

    [HttpPost("{documentId}")]
    [ProducesResponseType(typeof(LikeStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LikeStatusDto>> ToggleLike(long documentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _likeService.ToggleLikeAsync(userId, documentId);
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

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
