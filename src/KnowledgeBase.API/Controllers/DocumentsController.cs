using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.Documents;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace KnowledgeBase.API.Controllers;

[ApiController]
[Route("documents")]
[Authorize]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IDocumentService documentService,
        IServiceScopeFactory scopeFactory,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<DocumentListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DocumentListDto>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null,
        [FromQuery] long? categoryId = null,
        [FromQuery] int? status = null,
        [FromQuery] string? tag = null)
    {
        var request = new DocumentPagedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Keyword = keyword,
            CategoryId = categoryId,
            Status = status,
            Tag = tag
        };
        var userId = GetCurrentUserIdOrNull();
        var userRole = GetCurrentUserRoleOrNull();
        var applyVisibilityFilter = !status.HasValue || status.Value == (int)DocumentStatus.Published;
        var result = await _documentService.GetPagedAsync(request, userId, userRole, applyVisibilityFilter);
        return Ok(result);
    }

    [HttpGet("tags/cloud")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<TagCloudDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TagCloudDto>>> GetTagCloud()
    {
        var userId = GetCurrentUserIdOrNull();
        var userRole = GetCurrentUserRoleOrNull();
        var result = await _documentService.GetTagCloudAsync(userId, userRole, applyVisibilityFilter: true);
        return Ok(result);
    }

    [HttpGet("tags/search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> SearchTags(
        [FromQuery] string keyword,
        [FromQuery] int limit = 10)
    {
        var userId = GetCurrentUserIdOrNull();
        var userRole = GetCurrentUserRoleOrNull();
        var result = await _documentService.SearchTagsAsync(keyword, limit, userId, userRole, applyVisibilityFilter: true);
        return Ok(result);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<DocumentListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DocumentListDto>>> Search(
        [FromQuery] string keyword,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserIdOrNull();
        var userRole = GetCurrentUserRoleOrNull();
        var result = await _documentService.SearchAsync(keyword, pageNumber, pageSize, userId, userRole);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDto>> GetById(long id)
    {
        try
        {
            var userId = GetCurrentUserIdOrNull();
            var userRole = GetCurrentUserRoleOrNull();
            var document = await _documentService.GetByIdAsync(id, userId, userRole);
            
            if (userId.HasValue && userId.Value > 0)
            {
                _ = RecordViewHistoryAsync(userId.Value, id);
            }
            
            return Ok(document);
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

    private async Task RecordViewHistoryAsync(long userId, long documentId)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var viewHistoryService = scope.ServiceProvider.GetRequiredService<IViewHistoryService>();
            await viewHistoryService.RecordViewAsync(userId, documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "异步记录浏览历史失败，UserId: {UserId}, DocumentId: {DocumentId}", userId, documentId);
        }
    }

    [HttpPost]
    [Authorize(Policy = "EditorOrAdmin")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentDto>> Create([FromBody] CreateDocumentRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var document = await _documentService.CreateAsync(request, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "EditorOrAdmin")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentDto>> Update(long id, [FromBody] UpdateDocumentRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var document = await _documentService.UpdateAsync(id, request, currentUserId);
            return Ok(document);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "EditorOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _documentService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/view")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> IncrementViewCount(long id)
    {
        await _documentService.IncrementViewCountAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    [Authorize(Policy = "EditorOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] int status)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            await _documentService.UpdateStatusAsync(id, status, currentUserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/visibility")]
    [Authorize(Policy = "EditorOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVisibility(long id, [FromBody] UpdateVisibilityRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            await _documentService.UpdateVisibilityAsync(id, request, currentUserId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("batch/status")]
    [Authorize(Policy = "EditorOrAdmin")]
    [ProducesResponseType(typeof(BatchOperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchOperationResult>> BatchUpdateStatus([FromBody] BatchUpdateStatusRequest request)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return BadRequest(new { message = "请选择要操作的文档" });
        }

        var currentUserId = GetCurrentUserId();
        var result = await _documentService.BatchUpdateStatusAsync(request.Ids, request.Status, currentUserId);
        return Ok(result);
    }

    [HttpPost("batch/visibility")]
    [Authorize(Policy = "EditorOrAdmin")]
    [ProducesResponseType(typeof(BatchOperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchOperationResult>> BatchUpdateVisibility([FromBody] BatchUpdateVisibilityRequest request)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return BadRequest(new { message = "请选择要操作的文档" });
        }

        var currentUserId = GetCurrentUserId();
        var visibilityRequest = new UpdateVisibilityRequest
        {
            Visibility = request.Visibility,
            AllowedRoles = request.AllowedRoles
        };
        var result = await _documentService.BatchUpdateVisibilityAsync(request.Ids, visibilityRequest, currentUserId);
        return Ok(result);
    }

    [HttpPost("batch/category")]
    [Authorize(Policy = "EditorOrAdmin")]
    [ProducesResponseType(typeof(BatchOperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchOperationResult>> BatchMoveCategory([FromBody] BatchMoveCategoryRequest request)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return BadRequest(new { message = "请选择要操作的文档" });
        }

        var currentUserId = GetCurrentUserId();
        var result = await _documentService.BatchMoveCategoryAsync(request.Ids, request.CategoryId, currentUserId);
        return Ok(result);
    }

    [HttpPost("batch/delete")]
    [Authorize(Policy = "EditorOrAdmin")]
    [ProducesResponseType(typeof(BatchOperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchOperationResult>> BatchDelete([FromBody] BatchDeleteRequest request)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            return BadRequest(new { message = "请选择要删除的文档" });
        }

        var result = await _documentService.BatchDeleteAsync(request.Ids);
        return Ok(result);
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.Parse(userIdClaim ?? "0");
    }

    private long? GetCurrentUserIdOrNull()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId) && userId > 0)
        {
            return userId;
        }
        return null;
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
