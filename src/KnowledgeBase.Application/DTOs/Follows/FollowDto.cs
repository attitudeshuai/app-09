namespace KnowledgeBase.Application.DTOs.Follows;

public class FollowDto
{
    public long FollowerId { get; set; }
    public long FollowingId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FollowStatusDto
{
    public long UserId { get; set; }
    public bool IsFollowing { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
}

public class FollowerUserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public DateTime FollowedAt { get; set; }
}

public class FollowingUserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public DateTime FollowedAt { get; set; }
}
