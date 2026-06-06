using KnowledgeBase.Application.DTOs.Statistics;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;

namespace KnowledgeBase.Application.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IUnitOfWork _unitOfWork;

    public StatisticsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StatisticsDto> GetOverviewAsync()
    {
        var allDocuments = await _unitOfWork.Documents.GetAllAsync();
        var documents = allDocuments.ToList();
        var allUsers = await _unitOfWork.Users.GetAllAsync();
        var allCategories = await _unitOfWork.Categories.GetAllAsync();
        var allVersions = await _unitOfWork.DocumentVersions.GetAllAsync();

        return new StatisticsDto
        {
            TotalUsers = allUsers.Count(),
            TotalCategories = allCategories.Count(),
            TotalDocuments = documents.Count,
            PublishedDocuments = documents.Count(d => d.Status == DocumentStatus.Published),
            DraftDocuments = documents.Count(d => d.Status == DocumentStatus.Draft),
            ArchivedDocuments = documents.Count(d => d.Status == DocumentStatus.Archived),
            TotalDocumentVersions = allVersions.Count(),
            TotalViews = documents.Sum(d => d.ViewCount)
        };
    }

    public async Task<IEnumerable<CategoryDocumentCountDto>> GetCategoryDocumentCountsAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        var result = new List<CategoryDocumentCountDto>();

        foreach (var category in categories)
        {
            var documents = await _unitOfWork.Documents.GetByCategoryIdAsync(category.Id);
            result.Add(new CategoryDocumentCountDto
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
                DocumentCount = documents.Count()
            });
        }

        return result.OrderByDescending(x => x.DocumentCount);
    }
}
