using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.Favorites;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KnowledgeBase.API.Controllers;

[ApiController]
[Route("favorites")]
[Authorize]
[Produces("application/json")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FavoriteDocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FavoriteDocumentDto>>> GetMyFavorites(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRoleOrNull();
        var request = new FavoritePagedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _favoriteService.GetMyFavoritesAsync(userId, request, userRole);
        return Ok(result);
    }

    [HttpGet("{documentId}/status")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> GetFavoriteStatus(long documentId)
    {
        var userId = GetCurrentUserId();
        var isFavorited = await _favoriteService.IsFavoritedAsync(userId, documentId);
        return Ok(isFavorited);
    }

    [HttpPost("{documentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddFavorite(long documentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRoleOrNull();
            await _favoriteService.AddFavoriteAsync(userId, documentId, userRole);
            return NoContent();
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

    [HttpDelete("{documentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveFavorite(long documentId)
    {
        var userId = GetCurrentUserId();
        await _favoriteService.RemoveFavoriteAsync(userId, documentId);
        return NoContent();
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.Parse(userIdClaim ?? "0");
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
