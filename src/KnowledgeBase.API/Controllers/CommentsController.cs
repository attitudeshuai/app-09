using KnowledgeBase.Application.DTOs.Comments;
using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KnowledgeBase.API.Controllers;

[ApiController]
[Route("comments")]
[Produces("application/json")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("document/{documentId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<CommentDto>>> GetPagedByDocumentId(
        long documentId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var request = new CommentPagedRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _commentService.GetPagedByDocumentIdAsync(documentId, request);
        return Ok(result);
    }

    [HttpGet("document/{documentId}/count")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetCommentCount(long documentId)
    {
        var count = await _commentService.GetCountByDocumentIdAsync(documentId);
        return Ok(count);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> Create([FromBody] CreateCommentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var comment = await _commentService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetPagedByDocumentId), new { documentId = comment.DocumentId }, comment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.Parse(userIdClaim ?? "0");
    }
}
