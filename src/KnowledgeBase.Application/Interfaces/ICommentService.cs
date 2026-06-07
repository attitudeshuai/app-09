using KnowledgeBase.Application.DTOs.Comments;
using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Application.Interfaces;

public interface ICommentService
{
    Task<PagedResult<CommentDto>> GetPagedByDocumentIdAsync(long documentId, CommentPagedRequest request, bool isAuthenticated = false, UserRole? userRole = null);
    Task<CommentDto> CreateAsync(CreateCommentRequest request, long userId, UserRole? userRole = null);
    Task<int> GetCountByDocumentIdAsync(long documentId, bool isAuthenticated = false, UserRole? userRole = null);
    Task<List<CommentDto>> GetRepliesByParentIdAsync(long parentId, CommentSortOrder sortOrder = CommentSortOrder.Asc);
}
