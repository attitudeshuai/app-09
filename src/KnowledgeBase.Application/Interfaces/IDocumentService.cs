using KnowledgeBase.Application.DTOs.Documents;
using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentDto> GetByIdAsync(long id, long? userId = null, UserRole? userRole = null);
    Task<PagedResult<DocumentListDto>> GetPagedAsync(DocumentPagedRequest request, long? userId = null, UserRole? userRole = null, bool applyVisibilityFilter = false);
    Task<PagedResult<DocumentListDto>> SearchAsync(string keyword, int pageNumber, int pageSize, long? userId = null, UserRole? userRole = null);
    Task<DocumentDto> CreateAsync(CreateDocumentRequest request, long currentUserId);
    Task<DocumentDto> UpdateAsync(long id, UpdateDocumentRequest request, long currentUserId);
    Task DeleteAsync(long id);
    Task IncrementViewCountAsync(long id);
    Task UpdateStatusAsync(long id, int status, long currentUserId);
    Task UpdateVisibilityAsync(long id, UpdateVisibilityRequest request, long currentUserId);
    Task<int> PublishScheduledDocumentsAsync();
    Task<BatchOperationResult> BatchUpdateStatusAsync(List<long> ids, int status, long currentUserId);
    Task<BatchOperationResult> BatchUpdateVisibilityAsync(List<long> ids, UpdateVisibilityRequest request, long currentUserId);
    Task<BatchOperationResult> BatchMoveCategoryAsync(List<long> ids, long categoryId, long currentUserId);
    Task<BatchOperationResult> BatchDeleteAsync(List<long> ids);
    Task<IEnumerable<TagCloudDto>> GetTagCloudAsync(long? userId = null, UserRole? userRole = null, bool applyVisibilityFilter = false);
    Task<IEnumerable<string>> SearchTagsAsync(string keyword, int limit = 10, long? userId = null, UserRole? userRole = null, bool applyVisibilityFilter = false);
}
