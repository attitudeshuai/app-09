using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Application.DTOs.Badges;

public class BadgeDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BadgeType Type { get; set; }
    public int? RequiredValue { get; set; }
    public bool IsActive { get; set; }
}

public class UserBadgeDto
{
    public long BadgeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BadgeType Type { get; set; }
    public DateTime EarnedAt { get; set; }
}

public class CreateBadgeRequest
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BadgeType Type { get; set; }
    public int? RequiredValue { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateBadgeRequest
{
    public string? Name { get; set; }
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public BadgeType? Type { get; set; }
    public int? RequiredValue { get; set; }
    public bool? IsActive { get; set; }
}
