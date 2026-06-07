using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.Favorites;
using KnowledgeBase.Application.Interfaces;
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
        var request = new FavoritePagedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _favoriteService.GetMyFavoritesAsync(userId, request);
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
            await _favoriteService.AddFavoriteAsync(userId, documentId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
}
