namespace KnowledgeBase.Domain.Entities;

public class Document : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Tags { get; set; }
    public long CategoryId { get; set; }
    public Category? Category { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Draft;
    public int ViewCount { get; set; }
    public int Version { get; set; } = 1;
    public DateTime? PublishTime { get; set; }
    public bool IsAutoPublished { get; set; }
    public long? ScheduledBy { get; set; }
    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
}
