namespace KnowledgeBase.Domain.Entities;

public class Badge : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BadgeType Type { get; set; }
    public int? RequiredValue { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum BadgeType
{
    Points = 1,
    Documents = 2,
    Likes = 3,
    Comments = 4,
    Followers = 5
}
