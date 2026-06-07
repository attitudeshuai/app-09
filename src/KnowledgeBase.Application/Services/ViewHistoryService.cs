using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.ViewHistories;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;

namespace KnowledgeBase.Application.Services;

public class ViewHistoryService : IViewHistoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public ViewHistoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task RecordViewAsync(long userId, long documentId)
    {
        if (userId <= 0 || documentId <= 0)
        {
            return;
        }

        if (!await _unitOfWork.Documents.ExistsAsync(documentId))
        {
            return;
        }

        await _unitOfWork.DocumentViewHistories.AddOrUpdateAsync(userId, documentId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PagedResult<ViewHistoryDocumentDto>> GetMyViewHistoryAsync(long userId, ViewHistoryPagedRequest request)
    {
        var (items, totalCount) = await _unitOfWork.DocumentViewHistories.GetHistoryByUserIdAsync(
            userId, request.PageNumber, request.PageSize);

        return new PagedResult<ViewHistoryDocumentDto>
        {
            Items = items.Select(MapToViewHistoryDocumentDto),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<int> GetViewHistoryCountAsync(long userId)
    {
        return await _unitOfWork.DocumentViewHistories.GetHistoryCountByUserIdAsync(userId);
    }

    private static ViewHistoryDocumentDto MapToViewHistoryDocumentDto(DocumentViewHistory history)
    {
        return new ViewHistoryDocumentDto
        {
            Id = history.Document!.Id,
            Title = history.Document.Title,
            Summary = history.Document.Summary,
            Tags = history.Document.Tags,
            CategoryId = history.Document.CategoryId,
            CategoryName = history.Document.Category?.Name,
            Status = (int)history.Document.Status,
            ViewCount = history.Document.ViewCount,
            Version = history.Document.Version,
            CreatedAt = history.Document.CreatedAt,
            UpdatedAt = history.Document.UpdatedAt,
            ViewedAt = history.ViewedAt
        };
    }
}
