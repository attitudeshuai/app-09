using KnowledgeBase.Application.DTOs.Statistics;

namespace KnowledgeBase.Application.Interfaces;

public interface IStatisticsService
{
    Task<StatisticsDto> GetOverviewAsync();
    Task<IEnumerable<CategoryDocumentCountDto>> GetCategoryDocumentCountsAsync();
}
