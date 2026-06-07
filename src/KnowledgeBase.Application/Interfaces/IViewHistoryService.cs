using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.DTOs.ViewHistories;

namespace KnowledgeBase.Application.Interfaces;

public interface IViewHistoryService
{
    Task RecordViewAsync(long userId, long documentId);
    Task<PagedResult<ViewHistoryDocumentDto>> GetMyViewHistoryAsync(long userId, ViewHistoryPagedRequest request);
    Task<int> GetViewHistoryCountAsync(long userId);
}
