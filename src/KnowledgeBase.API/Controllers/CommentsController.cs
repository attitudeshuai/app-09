using KnowledgeBase.Application.DTOs.Comments;
using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
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
        [FromQuery] int pageSize = 20,
        [FromQuery] CommentSortOrder sortOrder = CommentSortOrder.Asc)
    {
        try
        {
            var request = new CommentPagedRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortOrder = sortOrder
            };
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var userRole = GetCurrentUserRoleOrNull();
            var result = await _commentService.GetPagedByDocumentIdAsync(documentId, request, isAuthenticated, userRole);
            return Ok(result);
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

    [HttpGet("{parentId}/replies")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CommentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CommentDto>>> GetReplies(
        long parentId,
        [FromQuery] CommentSortOrder sortOrder = CommentSortOrder.Asc)
    {
        var result = await _commentService.GetRepliesByParentIdAsync(parentId, sortOrder);
        return Ok(result);
    }

    [HttpGet("document/{documentId}/count")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> GetCommentCount(long documentId)
    {
        try
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var userRole = GetCurrentUserRoleOrNull();
            var count = await _commentService.GetCountByDocumentIdAsync(documentId, isAuthenticated, userRole);
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
            var userRole = GetCurrentUserRoleOrNull();
            var comment = await _commentService.CreateAsync(request, userId, userRole);
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
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
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
