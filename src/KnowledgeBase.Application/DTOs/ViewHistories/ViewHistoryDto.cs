namespace KnowledgeBase.Application.DTOs.ViewHistories;

public class ViewHistoryDocumentDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Tags { get; set; }
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int Status { get; set; }
    public int ViewCount { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime ViewedAt { get; set; }
}

public class ViewHistoryPagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
