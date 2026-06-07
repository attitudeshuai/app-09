namespace KnowledgeBase.Domain.Entities;

public class Level : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int LevelNumber { get; set; }
    public int RequiredPoints { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }
}
