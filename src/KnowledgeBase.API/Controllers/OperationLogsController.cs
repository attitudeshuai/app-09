using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.OperationLogs;
using KnowledgeBase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KnowledgeBase.API.Controllers;

[ApiController]
[Route("operation-logs")]
[Authorize(Policy = "EditorOrAdmin")]
[Produces("application/json")]
public class OperationLogsController : ControllerBase
{
    private readonly IOperationLogService _operationLogService;

    public OperationLogsController(IOperationLogService operationLogService)
    {
        _operationLogService = operationLogService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OperationLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OperationLogDto>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? userId = null,
        [FromQuery] string? actionType = null,
        [FromQuery] string? targetType = null,
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null)
    {
        var request = new OperationLogPagedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            UserId = userId,
            ActionType = actionType,
            TargetType = targetType,
            StartTime = startTime,
            EndTime = endTime
        };
        var result = await _operationLogService.GetPagedAsync(request);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Create([FromBody] CreateOperationLogRequest request)
    {
        var userId = GetCurrentUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        await _operationLogService.LogAsync(userId, request, ipAddress, userAgent);
        return NoContent();
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.Parse(userIdClaim ?? "0");
    }
}
