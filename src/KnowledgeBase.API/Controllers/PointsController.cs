using KnowledgeBase.Application.DTOs.Points;
using KnowledgeBase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KnowledgeBase.API.Controllers;

[ApiController]
[Route("points")]
[Authorize]
[Produces("application/json")]
public class PointsController : ControllerBase
{
    private readonly IPointService _pointService;

    public PointsController(IPointService pointService)
    {
        _pointService = pointService;
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(UserPointSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserPointSummaryDto>> GetMySummary()
    {
        try
        {
            var userId = GetCurrentUserId();
            var summary = await _pointService.GetUserPointSummaryAsync(userId);
            return Ok(summary);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("records")]
    [ProducesResponseType(typeof(List<PointRecordDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PointRecordDto>>> GetMyRecords(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var request = new PointPagedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var records = await _pointService.GetPointRecordsAsync(userId, request);
        var totalCount = await _pointService.GetPointRecordCountAsync(userId);
        return Ok(new { items = records, totalCount, pageNumber, pageSize });
    }

    [HttpGet("user/{userId}/summary")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserPointSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserPointSummaryDto>> GetUserSummary(long userId)
    {
        try
        {
            var summary = await _pointService.GetUserPointSummaryAsync(userId);
            return Ok(summary);
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
