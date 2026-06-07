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
}

public class UpdateProfileRequest
{
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
}
