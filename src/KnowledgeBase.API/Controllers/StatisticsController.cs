using KnowledgeBase.Application.DTOs.Statistics;
using KnowledgeBase.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBase.API.Controllers;

[ApiController]
[Route("statistics")]
[Authorize]
[Produces("application/json")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [HttpGet("overview")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(StatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StatisticsDto>> GetOverview()
    {
        var statistics = await _statisticsService.GetOverviewAsync();
        return Ok(statistics);
    }

    [HttpGet("category-document-counts")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CategoryDocumentCountDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryDocumentCountDto>>> GetCategoryDocumentCounts()
    {
        var result = await _statisticsService.GetCategoryDocumentCountsAsync();
        return Ok(result);
    }
}
