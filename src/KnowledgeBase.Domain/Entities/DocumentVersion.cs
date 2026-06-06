namespace KnowledgeBase.Domain.Entities;

public class DocumentVersion : BaseEntity
{
    public long DocumentId { get; set; }
    public Document? Document { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Tags { get; set; }
    public string? ChangeLog { get; set; }
}
