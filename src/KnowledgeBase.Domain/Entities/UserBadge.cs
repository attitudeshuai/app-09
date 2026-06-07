namespace KnowledgeBase.Domain.Entities;

public class UserBadge
{
    public long UserId { get; set; }
    public User? User { get; set; }
    public long BadgeId { get; set; }
    public Badge? Badge { get; set; }
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}
