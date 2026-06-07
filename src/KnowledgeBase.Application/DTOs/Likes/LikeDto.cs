namespace KnowledgeBase.Application.DTOs.Likes;

public class LikeDto
{
    public long UserId { get; set; }
    public long DocumentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LikeCountDto
{
    public long DocumentId { get; set; }
    public int Count { get; set; }
}

public class LikeStatusDto
{
    public long DocumentId { get; set; }
    public bool IsLiked { get; set; }
    public int LikeCount { get; set; }
}
