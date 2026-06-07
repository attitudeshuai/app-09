using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface IDocumentViewHistoryRepository
{
    Task AddOrUpdateAsync(long userId, long documentId);
    Task<(IEnumerable<DocumentViewHistory> Items, int TotalCount)> GetHistoryByUserIdAsync(
        long userId,
        int pageNumber,
        int pageSize);
    Task<int> GetHistoryCountByUserIdAsync(long userId);
    Task CleanupOldRecordsAsync(int maxRecordsPerUser, TimeSpan expirationPeriod);
    Task DeleteByDocumentIdAsync(long documentId);
}
