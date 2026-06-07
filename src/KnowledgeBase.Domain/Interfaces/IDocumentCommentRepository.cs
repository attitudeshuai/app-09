using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface IDocumentCommentRepository : IRepository<DocumentComment>
{
    Task<(IEnumerable<DocumentComment> Items, int TotalCount)> GetPagedByDocumentIdAsync(
        long documentId,
        int pageNumber,
        int pageSize,
        bool sortDescending = false);
    Task<List<DocumentComment>> GetAllByDocumentIdAsync(long documentId, bool sortDescending = false);
    Task<List<DocumentComment>> GetRepliesByParentIdAsync(long parentId, bool sortDescending = false);
    Task<int> CountByDocumentIdAsync(long documentId);
    Task<int> CountRootCommentsByDocumentIdAsync(long documentId);
}
