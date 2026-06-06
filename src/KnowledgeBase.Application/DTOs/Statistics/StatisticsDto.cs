namespace KnowledgeBase.Application.DTOs.Statistics;

public class StatisticsDto
{
    public int TotalUsers { get; set; }
    public int TotalCategories { get; set; }
    public int TotalDocuments { get; set; }
    public int PublishedDocuments { get; set; }
    public int DraftDocuments { get; set; }
    public int ArchivedDocuments { get; set; }
    public int TotalDocumentVersions { get; set; }
    public long TotalViews { get; set; }
}

public class CategoryDocumentCountDto
{
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int DocumentCount { get; set; }
}
