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
    public int Visibility { get; set; }
    public string? AllowedRoles { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int Version { get; set; }
    public DateTime? PublishTime { get; set; }
    public bool IsAutoPublished { get; set; }
    public long? ScheduledBy { get; set; }
    public long CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsFavorited { get; set; }
    public bool IsLiked { get; set; }
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
    public int Visibility { get; set; }
    public string? AllowedRoles { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int Version { get; set; }
    public DateTime? PublishTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsFavorited { get; set; }
    public bool IsLiked { get; set; }
}

public class CreateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Tags { get; set; }
    public long CategoryId { get; set; }
    public int Status { get; set; } = 1;
    public int Visibility { get; set; } = 1;
    public string? AllowedRoles { get; set; }
    public DateTime? PublishTime { get; set; }
}

public class UpdateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Tags { get; set; }
    public long CategoryId { get; set; }
    public int? Status { get; set; }
    public int? Visibility { get; set; }
    public string? AllowedRoles { get; set; }
    public DateTime? PublishTime { get; set; }
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

public class BatchOperationResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<long> SuccessIds { get; set; } = new();
    public List<BatchFailureItem> Failures { get; set; } = new();
}

public class BatchFailureItem
{
    public long Id { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BatchUpdateStatusRequest
{
    public List<long> Ids { get; set; } = new();
    public int Status { get; set; }
}

public class BatchMoveCategoryRequest
{
    public List<long> Ids { get; set; } = new();
    public long CategoryId { get; set; }
}

public class BatchDeleteRequest
{
    public List<long> Ids { get; set; } = new();
}

public class UpdateVisibilityRequest
{
    public int Visibility { get; set; }
    public string? AllowedRoles { get; set; }
}

public class BatchUpdateVisibilityRequest
{
    public List<long> Ids { get; set; } = new();
    public int Visibility { get; set; }
    public string? AllowedRoles { get; set; }
}
