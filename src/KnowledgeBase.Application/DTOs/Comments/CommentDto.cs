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
}

public class CreateCommentRequest
{
    public long DocumentId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class CommentPagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
