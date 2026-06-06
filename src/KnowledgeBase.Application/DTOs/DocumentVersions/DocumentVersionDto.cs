namespace KnowledgeBase.Application.DTOs.DocumentVersions;

public class DocumentVersionDto
{
    public long Id { get; set; }
    public long DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Tags { get; set; }
    public string? ChangeLog { get; set; }
    public long CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RestoreVersionRequest
{
    public string? ChangeLog { get; set; }
}
