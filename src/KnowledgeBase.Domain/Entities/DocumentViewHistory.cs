namespace KnowledgeBase.Domain.Entities;

public class DocumentViewHistory
{
    public long UserId { get; set; }
    public User? User { get; set; }
    public long DocumentId { get; set; }
    public Document? Document { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}
