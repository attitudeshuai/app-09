namespace KnowledgeBase.Application.DTOs.Documents;

public class DocumentDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Tags { get; set; }
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int Status { get; set; }
    public int ViewCount { get; set; }
    public int Version { get; set; }
    public long CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DocumentListDto
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
}

public class CreateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Tags { get; set; }
    public long CategoryId { get; set; }
    public int Status { get; set; } = 1;
}

public class UpdateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Tags { get; set; }
    public long CategoryId { get; set; }
    public int? Status { get; set; }
    public string? ChangeLog { get; set; }
}

public class DocumentPagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public long? CategoryId { get; set; }
    public int? Status { get; set; }
}
