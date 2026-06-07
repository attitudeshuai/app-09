using KnowledgeBase.Application.DTOs.Documents;
using KnowledgeBase.Application.DTOs.Common;

namespace KnowledgeBase.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentDto> GetByIdAsync(long id, long? userId = null);
    Task<PagedResult<DocumentListDto>> GetPagedAsync(DocumentPagedRequest request, long? userId = null);
    Task<PagedResult<DocumentListDto>> SearchAsync(string keyword, int pageNumber, int pageSize, long? userId = null);
    Task<DocumentDto> CreateAsync(CreateDocumentRequest request, long currentUserId);
    Task<DocumentDto> UpdateAsync(long id, UpdateDocumentRequest request, long currentUserId);
    Task DeleteAsync(long id);
    Task IncrementViewCountAsync(long id);
    Task UpdateStatusAsync(long id, int status, long currentUserId);
}
