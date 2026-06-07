namespace KnowledgeBase.Application.DTOs.Comments;

public class CommentDto
{
    public long Id { get; set; }
    public long DocumentId { get; set; }
    public long UserId { get; set; }
    public string? Username { get; set; }
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long? ParentId { get; set; }
    public long? ReplyToUserId { get; set; }
    public string? ReplyToUsername { get; set; }
    public string? ReplyToNickname { get; set; }
    public string? ReplyToAvatar { get; set; }
    public int ReplyCount { get; set; }
    public List<CommentDto> Replies { get; set; } = new();
}

public class CreateCommentRequest
{
    public long DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public long? ReplyToUserId { get; set; }
}

public class CommentPagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public CommentSortOrder SortOrder { get; set; } = CommentSortOrder.Asc;
}

public enum CommentSortOrder
{
    Asc,
    Desc
}
