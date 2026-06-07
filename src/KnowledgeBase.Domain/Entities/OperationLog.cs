namespace KnowledgeBase.Domain.Entities;

public class OperationLog
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User? User { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public long? TargetId { get; set; }
    public string? TargetName { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
