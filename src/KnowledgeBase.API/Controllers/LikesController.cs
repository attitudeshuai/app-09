using KnowledgeBase.Application.DTOs.Likes;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LikeStatusDto>> GetLikeStatus(long documentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var userRole = GetCurrentUserRoleOrNull();
            var status = await _likeService.GetLikeStatusAsync(userId, documentId, isAuthenticated, userRole);
            return Ok(status);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("{documentId}/count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> GetLikeCount(long documentId)
    {
        try
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var userRole = GetCurrentUserRoleOrNull();
            var count = await _likeService.GetLikeCountAsync(documentId, isAuthenticated, userRole);
            return Ok(count);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
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
            var userRole = GetCurrentUserRoleOrNull();
            var result = await _likeService.ToggleLikeAsync(userId, documentId, userRole);
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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private UserRole? GetCurrentUserRoleOrNull()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
        if (Enum.TryParse<UserRole>(roleClaim, out var role))
        {
            return role;
        }
        return null;
    }
}
