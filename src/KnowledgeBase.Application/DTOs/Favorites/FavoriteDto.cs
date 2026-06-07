namespace KnowledgeBase.Application.DTOs.Favorites;

public class FavoriteDto
{
    public long UserId { get; set; }
    public long DocumentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FavoriteDocumentDto
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
    public DateTime FavoritedAt { get; set; }
}

public class FavoritePagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
