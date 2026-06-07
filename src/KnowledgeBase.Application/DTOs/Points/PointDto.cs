using KnowledgeBase.Domain.Entities;

namespace KnowledgeBase.Application.DTOs.Points;

public class PointRecordDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public int Points { get; set; }
    public PointSource Source { get; set; }
    public string? SourceText { get; set; }
    public string? Description { get; set; }
    public long? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PointPagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class UserPointSummaryDto
{
    public long UserId { get; set; }
    public int TotalPoints { get; set; }
    public int LevelNumber { get; set; }
    public string? LevelName { get; set; }
    public string? LevelIcon { get; set; }
    public string? LevelColor { get; set; }
    public int PointsToNextLevel { get; set; }
    public int NextLevelRequiredPoints { get; set; }
}
