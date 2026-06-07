namespace KnowledgeBase.Application.DTOs.OperationLogs;

public class OperationLogDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string? Username { get; set; }
    public string? Nickname { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public long? TargetId { get; set; }
    public string? TargetName { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? IpAddress { get; set; }
}

public class OperationLogPagedRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public long? UserId { get; set; }
    public string? ActionType { get; set; }
    public string? TargetType { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

public class CreateOperationLogRequest
{
    public string ActionType { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public long? TargetId { get; set; }
    public string? TargetName { get; set; }
    public string? Details { get; set; }
}
