using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface IDocumentCommentRepository : IRepository<DocumentComment>
{
    Task<(IEnumerable<DocumentComment> Items, int TotalCount)> GetPagedByDocumentIdAsync(
        long documentId,
        int pageNumber,
        int pageSize);
    Task<int> CountByDocumentIdAsync(long documentId);
}
