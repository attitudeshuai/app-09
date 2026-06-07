using KnowledgeBase.Application.DTOs.Comments;
using KnowledgeBase.Application.DTOs.Common;
using KnowledgeBase.Application.Interfaces;
using KnowledgeBase.Domain.Entities;
using KnowledgeBase.Domain.Interfaces;

namespace KnowledgeBase.Application.Services;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;

    public CommentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CommentDto>> GetPagedByDocumentIdAsync(long documentId, CommentPagedRequest request)
    {
        var (items, totalCount) = await _unitOfWork.DocumentComments.GetPagedByDocumentIdAsync(
            documentId, request.PageNumber, request.PageSize);

        return new PagedResult<CommentDto>
        {
            Items = items.Select(MapToCommentDto),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<CommentDto> CreateAsync(CreateCommentRequest request, long userId)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("评论内容不能为空");
        }

        var document = await _unitOfWork.Documents.GetByIdAsync(request.DocumentId);
        if (document == null)
        {
            throw new KeyNotFoundException("文档不存在");
        }

        if (document.Status != DocumentStatus.Published)
        {
            throw new InvalidOperationException("只有已发布的文档才能发表评论");
        }

        var comment = new DocumentComment
        {
            DocumentId = request.DocumentId,
            UserId = userId,
            Content = request.Content.Trim(),
            CreatedBy = userId
        };

        await _unitOfWork.DocumentComments.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();

        var createdComment = await _unitOfWork.DocumentComments.GetByIdAsync(comment.Id);
        return MapToCommentDto(createdComment!);
    }

    public async Task<int> GetCountByDocumentIdAsync(long documentId)
    {
        return await _unitOfWork.DocumentComments.CountByDocumentIdAsync(documentId);
    }

    private static CommentDto MapToCommentDto(DocumentComment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            DocumentId = comment.DocumentId,
            UserId = comment.UserId,
            Username = comment.User?.Username,
            Nickname = comment.User?.Nickname,
            Avatar = comment.User?.Avatar,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }
}
