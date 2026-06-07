using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.ViewHistories;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace KnowledgeBase.Application.Services;

public class ViewHistoryService : IViewHistoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ViewHistoryService> _logger;

    public ViewHistoryService(IUnitOfWork unitOfWork, ILogger<ViewHistoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task RecordViewAsync(long userId, long documentId)
    {
        if (userId <= 0 || documentId <= 0)
        {
            return;
        }

        try
        {
            if (!await _unitOfWork.Documents.ExistsAsync(documentId))
            {
                _logger.LogWarning("尝试记录浏览历史时文档不存在，DocumentId: {DocumentId}, UserId: {UserId}", documentId, userId);
                return;
            }

            await _unitOfWork.DocumentViewHistories.AddOrUpdateAsync(userId, documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录浏览历史失败，UserId: {UserId}, DocumentId: {DocumentId}", userId, documentId);
            throw;
        }
    }

    public async Task<PagedResult<ViewHistoryDocumentDto>> GetMyViewHistoryAsync(long userId, ViewHistoryPagedRequest request)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取浏览历史失败，UserId: {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetViewHistoryCountAsync(long userId)
    {
        try
        {
            return await _unitOfWork.DocumentViewHistories.GetHistoryCountByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取浏览历史数量失败，UserId: {UserId}", userId);
            throw;
        }
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
