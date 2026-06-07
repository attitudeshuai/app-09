namespace KnowledgeBase.Application.Options;

public class ViewHistoryOptions
{
    public const string SectionName = "ViewHistory";

    public int MaxRecordsPerUser { get; set; } = 100;
    public int ExpirationDays { get; set; } = 30;
    public int CleanupIntervalHours { get; set; } = 24;
}
