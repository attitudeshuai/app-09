using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.Documents;
using KnowledgeBase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KnowledgeBase.API.Controllers;

[ApiController]
[Route("documents")]
[Authorize]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IViewHistoryService _viewHistoryService;

    public DocumentsController(IDocumentService documentService, IViewHistoryService viewHistoryService)
    {
        _documentService = documentService;
        _viewHistoryService = viewHistoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<DocumentListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DocumentListDto>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null,
        [FromQuery] long? categoryId = null,
        [FromQuery] int? status = null)
    {
        var request = new DocumentPagedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Keyword = keyword,
            CategoryId = categoryId,
            Status = status
        };
        var userId = GetCurrentUserIdOrNull();
        var result = await _documentService.GetPagedAsync(request, userId);
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
        var result = await _documentService.SearchAsync(keyword, pageNumber, pageSize, userId);
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
            var document = await _documentService.GetByIdAsync(id, userId);
            
            if (userId.HasValue && userId.Value > 0)
            {
                _ = _viewHistoryService.RecordViewAsync(userId.Value, id);
            }
            
            return Ok(document);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
}
