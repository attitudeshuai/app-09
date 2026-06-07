namespace KnowledgeBase.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Avatar { get; set; }
    public UserRole Role { get; set; } = UserRole.Viewer;
    public bool IsActive { get; set; } = true;
    public bool IsLockedOut { get; set; } = false;
    public DateTime? LockoutEnd { get; set; }

    public int TotalPoints { get; set; }
    public long? LevelId { get; set; }
    public Level? Level { get; set; }

    public ICollection<UserPasswordHistory> PasswordHistories { get; set; } = new List<UserPasswordHistory>();
    public ICollection<PointRecord> PointRecords { get; set; } = new List<PointRecord>();
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}
