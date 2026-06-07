using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Domain.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    Task<(IEnumerable<Document> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? keyword,
        long? categoryId,
        DocumentStatus? status,
        bool applyVisibilityFilter = false,
        bool isAuthenticated = false,
        UserRole? userRole = null);
    Task<IEnumerable<Document>> SearchAsync(string keyword, int pageNumber, int pageSize,
        bool isAuthenticated = false,
        UserRole? userRole = null);
    Task<int> SearchCountAsync(string keyword,
        bool isAuthenticated = false,
        UserRole? userRole = null);
    Task IncrementViewCountAsync(long id);
    Task<IEnumerable<Document>> GetByCategoryIdAsync(long categoryId);
    Task<int> UpdateCategoryIdAsync(long sourceCategoryId, long targetCategoryId, long updatedBy);
    Task<IEnumerable<Document>> GetScheduledDocumentsToPublishAsync(DateTime now);
    Task<int> GetDocumentCountByUserIdAsync(long userId, DocumentStatus? status = null);
    Task<bool> CanViewDocumentAsync(long documentId, bool isAuthenticated, UserRole? userRole);
}
