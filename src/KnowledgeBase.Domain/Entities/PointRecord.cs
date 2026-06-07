namespace KnowledgeBase.Domain.Entities;

public class PointRecord : BaseEntity
{
    public long UserId { get; set; }
    public User? User { get; set; }
    public int Points { get; set; }
    public PointSource Source { get; set; }
    public string? Description { get; set; }
    public long? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
}

public enum PointSource
{
    PublishDocument = 1,
    Comment = 2,
    LikedByOthers = 3,
    SystemReward = 4
}
