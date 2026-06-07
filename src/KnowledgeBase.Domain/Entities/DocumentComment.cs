namespace KnowledgeBase.Domain.Entities;

public class DocumentComment : BaseEntity
{
    public long DocumentId { get; set; }
    public Document? Document { get; set; }
    public long UserId { get; set; }
    public User? User { get; set; }
    public string Content { get; set; } = string.Empty;
}
