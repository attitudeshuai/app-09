using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.ViewHistories;
using KnowledgeBase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KnowledgeBase.API.Controllers;

[ApiController]
[Route("view-history")]
[Authorize]
[Produces("application/json")]
public class ViewHistoryController : ControllerBase
{
    private readonly IViewHistoryService _viewHistoryService;

    public ViewHistoryController(IViewHistoryService viewHistoryService)
    {
        _viewHistoryService = viewHistoryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ViewHistoryDocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ViewHistoryDocumentDto>>> GetMyViewHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var request = new ViewHistoryPagedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _viewHistoryService.GetMyViewHistoryAsync(userId, request);
        return Ok(result);
    }

    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetViewHistoryCount()
    {
        var userId = GetCurrentUserId();
        var count = await _viewHistoryService.GetViewHistoryCountAsync(userId);
        return Ok(count);
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.Parse(userIdClaim ?? "0");
    }
}
