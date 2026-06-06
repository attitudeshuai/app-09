using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    Task<(IEnumerable<Document> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? keyword,
        long? categoryId,
        DocumentStatus? status);
    Task<IEnumerable<Document>> SearchAsync(string keyword, int pageNumber, int pageSize);
    Task<int> SearchCountAsync(string keyword);
    Task IncrementViewCountAsync(long id);
    Task<IEnumerable<Document>> GetByCategoryIdAsync(long categoryId);
}
