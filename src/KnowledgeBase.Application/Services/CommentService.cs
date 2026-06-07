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

    public async Task<PagedResult<CommentDto>> GetPagedByDocumentIdAsync(long documentId, CommentPagedRequest request, bool isAuthenticated = false, UserRole? userRole = null)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new KeyNotFoundException("文档不存在");
        }

        if (document.Status == DocumentStatus.Published)
        {
            var canView = await _unitOfWork.Documents.CanViewDocumentAsync(documentId, isAuthenticated, userRole);
            if (!canView)
            {
                throw new UnauthorizedAccessException("无权查看该文档的评论");
            }
        }

        var sortDescending = request.SortOrder == CommentSortOrder.Desc;

        var (rootComments, totalCount) = await _unitOfWork.DocumentComments.GetPagedByDocumentIdAsync(
            documentId, request.PageNumber, request.PageSize, sortDescending);

        var allComments = await _unitOfWork.DocumentComments.GetAllByDocumentIdAsync(documentId, sortDescending: false);

        var commentDtos = rootComments.Select(c =>
        {
            var dto = MapToCommentDto(c);
            dto.Replies = BuildReplyTree(c.Id, allComments, request.SortOrder);
            dto.ReplyCount = CountAllReplies(c.Id, allComments);
            return dto;
        }).ToList();

        return new PagedResult<CommentDto>
        {
            Items = commentDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<CommentDto> CreateAsync(CreateCommentRequest request, long userId, UserRole? userRole = null)
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

        var canView = await _unitOfWork.Documents.CanViewDocumentAsync(request.DocumentId, true, userRole);
        if (!canView)
        {
            throw new UnauthorizedAccessException("无权评论该文档");
        }

        long? parentId = null;
        long? replyToUserId = null;

        if (request.ParentId.HasValue)
        {
            var parentComment = await _unitOfWork.DocumentComments.GetByIdAsync(request.ParentId.Value);
            if (parentComment == null)
            {
                throw new KeyNotFoundException("父评论不存在");
            }
            if (parentComment.DocumentId != request.DocumentId)
            {
                throw new ArgumentException("父评论不属于该文档");
            }
            parentId = request.ParentId.Value;

            if (request.ReplyToUserId.HasValue)
            {
                var replyToUser = await _unitOfWork.Users.GetByIdAsync(request.ReplyToUserId.Value);
                if (replyToUser == null)
                {
                    throw new KeyNotFoundException("被回复的用户不存在");
                }
                replyToUserId = request.ReplyToUserId.Value;
            }
            else
            {
                replyToUserId = parentComment.UserId;
            }
        }

        var comment = new DocumentComment
        {
            DocumentId = request.DocumentId,
            UserId = userId,
            Content = request.Content.Trim(),
            ParentId = parentId,
            ReplyToUserId = replyToUserId,
            CreatedBy = userId
        };

        await _unitOfWork.DocumentComments.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();

        var createdComment = await _unitOfWork.DocumentComments.GetByIdAsync(comment.Id);
        return MapToCommentDto(createdComment!);
    }

    public async Task<int> GetCountByDocumentIdAsync(long documentId, bool isAuthenticated = false, UserRole? userRole = null)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
        if (document == null)
        {
            throw new KeyNotFoundException("文档不存在");
        }

        if (document.Status == DocumentStatus.Published)
        {
            var canView = await _unitOfWork.Documents.CanViewDocumentAsync(documentId, isAuthenticated, userRole);
            if (!canView)
            {
                throw new UnauthorizedAccessException("无权查看该文档的评论数");
            }
        }

        return await _unitOfWork.DocumentComments.CountByDocumentIdAsync(documentId);
    }

    public async Task<List<CommentDto>> GetRepliesByParentIdAsync(long parentId, CommentSortOrder sortOrder = CommentSortOrder.Asc)
    {
        var sortDescending = sortOrder == CommentSortOrder.Desc;
        var replies = await _unitOfWork.DocumentComments.GetRepliesByParentIdAsync(parentId, sortDescending);
        return replies.Select(MapToCommentDto).ToList();
    }

    private static List<CommentDto> BuildReplyTree(long parentId, List<DocumentComment> allComments, CommentSortOrder sortOrder)
    {
        var replies = allComments
            .Where(c => c.ParentId == parentId)
            .Select(c =>
            {
                var dto = MapToCommentDto(c);
                dto.Replies = BuildReplyTree(c.Id, allComments, sortOrder);
                dto.ReplyCount = CountAllReplies(c.Id, allComments);
                return dto;
            });

        return sortOrder == CommentSortOrder.Desc
            ? replies.OrderByDescending(c => c.CreatedAt).ToList()
            : replies.OrderBy(c => c.CreatedAt).ToList();
    }

    private static int CountAllReplies(long parentId, List<DocumentComment> allComments)
    {
        var directReplies = allComments.Where(c => c.ParentId == parentId).ToList();
        var count = directReplies.Count;
        foreach (var reply in directReplies)
        {
            count += CountAllReplies(reply.Id, allComments);
        }
        return count;
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
            CreatedAt = comment.CreatedAt,
            ParentId = comment.ParentId,
            ReplyToUserId = comment.ReplyToUserId,
            ReplyToUsername = comment.ReplyToUser?.Username,
            ReplyToNickname = comment.ReplyToUser?.Nickname,
            ReplyToAvatar = comment.ReplyToUser?.Avatar
        };
    }
}
