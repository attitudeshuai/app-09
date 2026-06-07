namespace KnowledgeBase.Domain.Entities;

public class DocumentLike
{
    public long UserId { get; set; }
    public User? User { get; set; }
    public long DocumentId { get; set; }
    public Document? Document { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
