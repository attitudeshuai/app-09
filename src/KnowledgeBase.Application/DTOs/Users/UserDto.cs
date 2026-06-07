namespace KnowledgeBase.Application.DTOs.Users;

public class UserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public int Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalPoints { get; set; }
    public long? LevelId { get; set; }
    public int? LevelNumber { get; set; }
    public string? LevelName { get; set; }
    public string? LevelIcon { get; set; }
    public string? LevelColor { get; set; }
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public int Role { get; set; } = 3;
}

public class UpdateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public int? Role { get; set; }
    public bool? IsActive { get; set; }
}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UserPagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
}

public class UserProfileDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public int Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalDocuments { get; set; }
    public int PublishedDocuments { get; set; }
    public int DraftDocuments { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public bool IsFollowing { get; set; }
    public int TotalPoints { get; set; }
    public int LevelNumber { get; set; }
    public string? LevelName { get; set; }
    public string? LevelIcon { get; set; }
    public string? LevelColor { get; set; }
    public int PointsToNextLevel { get; set; }
    public int NextLevelRequiredPoints { get; set; }
    public List<BadgeSummaryDto>? Badges { get; set; }
}

public class BadgeSummaryDto
{
    public long BadgeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EarnedAt { get; set; }
}

public class UpdateProfileRequest
{
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
}
