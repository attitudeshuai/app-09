using KnowledgeBase.Application.DTOs.DocumentVersions;
using KnowledgeBase.Application.DTOs.Documents;
using KnowledgeBase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KnowledgeBase.API.Controllers;

[ApiController]
[Route("documents/{documentId}/versions")]
[Authorize]
[Produces("application/json")]
public class DocumentVersionsController : ControllerBase
{
    private readonly IDocumentVersionService _versionService;

    public DocumentVersionsController(IDocumentVersionService versionService)
    {
        _versionService = versionService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<DocumentVersionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<DocumentVersionDto>>> GetByDocumentId(long documentId)
    {
        try
        {
            var versions = await _versionService.GetByDocumentIdAsync(documentId);
            return Ok(versions);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{versionNumber}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DocumentVersionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentVersionDto>> GetByVersionNumber(long documentId, int versionNumber)
    {
        var version = await _versionService.GetByDocumentIdAndVersionAsync(documentId, versionNumber);
        if (version == null)
        {
            return NotFound(new { message = "版本不存在" });
        }
        return Ok(version);
    }

    [HttpPost("{versionNumber}/restore")]
    [Authorize(Policy = "EditorOrAdmin")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDto>> RestoreVersion(long documentId, int versionNumber, [FromBody] RestoreVersionRequest? request = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var document = await _versionService.RestoreVersionAsync(documentId, versionNumber, request?.ChangeLog, currentUserId);
            return Ok(document);
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
}
