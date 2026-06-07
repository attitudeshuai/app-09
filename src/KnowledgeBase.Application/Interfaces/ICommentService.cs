using KnowledgeBase.Application.DTOs.Comments;
using KnowledgeBase.Application.DTOs.Common;

namespace KnowledgeBase.Application.Interfaces;

public interface ICommentService
{
    Task<PagedResult<CommentDto>> GetPagedByDocumentIdAsync(long documentId, CommentPagedRequest request);
    Task<CommentDto> CreateAsync(CreateCommentRequest request, long userId);
    Task<int> GetCountByDocumentIdAsync(long documentId);
    Task<List<CommentDto>> GetRepliesByParentIdAsync(long parentId, CommentSortOrder sortOrder = CommentSortOrder.Asc);
}
